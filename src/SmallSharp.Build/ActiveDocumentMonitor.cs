using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;

namespace SmallSharp.Build
{
    class ActiveDocumentMonitor : MarshalByRefObject, IDisposable, IVsRunningDocTableEvents, IVsSelectionEvents
    {
        FileSystemWatcher watcher;
        readonly IServiceProvider services;

        IVsRunningDocumentTable? rdt;
        IVsMonitorSelection? selection;
        uint rdtCookie;
        uint selectionCookie;

        string launchProfilesPath;
        string userFile;
        string flagFile;
        Dictionary<string, string> startupFiles = new();

        public ActiveDocumentMonitor(string launchProfilesPath, string userFile, string flagFile, IServiceProvider services)
        {
            this.launchProfilesPath = launchProfilesPath;
            this.userFile = userFile;
            this.flagFile = flagFile;
            this.services = services;

            watcher = new FileSystemWatcher(Path.GetDirectoryName(launchProfilesPath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "launchSettings.json",
            };

            watcher.Changed += (_, _) => ReloadProfiles();
            watcher.Created += (_, _) => ReloadProfiles();
            watcher.EnableRaisingEvents = true;
            ReloadProfiles();
        }

        public void Start()
        {
            rdt = (IVsRunningDocumentTable)services.GetService(typeof(SVsRunningDocumentTable));
            if (rdt != null)
                rdt.AdviseRunningDocTableEvents(this, out rdtCookie);

            selection = (IVsMonitorSelection)services.GetService(typeof(SVsShellMonitorSelection));
            if (selection != null)
                selection.AdviseSelectionEvents(this, out selectionCookie);
        }

        public void Refresh(string launchProfiles, string userFile, string flagFile)
        {
            launchProfilesPath = launchProfiles;
            this.userFile = userFile;
            this.flagFile = flagFile;
            watcher.Path = Path.GetDirectoryName(launchProfiles);
            ReloadProfiles();
        }

        void ReloadProfiles()
        {
            if (!File.Exists(launchProfilesPath))
                return;

            var maxAttempts = 5;
            var exceptions = new List<Exception>();

            for (var i = 0; i < maxAttempts; i++)
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(launchProfilesPath));
                    if (json.Property("profiles") is not JProperty prop ||
                        prop.Value is not JObject profiles)
                        return;

                    startupFiles = profiles.Properties().Select(p => p.Name)
                        .ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);

                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    Thread.Sleep(500);
                }
            }

            // NOTE: check exceptions list to see why.
            Debug.Fail("Could not read launchSettings.json");
        }

        void UpdateStartupFile(string? path)
        {
            if (!string.IsNullOrEmpty(path) &&
                path!.IndexOfAny(Path.GetInvalidPathChars()) == -1 &&
                Path.GetFileName(path) is string startupFile &&
                startupFiles.ContainsKey(startupFile))
            {
                try
                {
                    // Get the value as it was exists in the original dictionary, 
                    // since it has to match what the source generator created in the 
                    // launch profiles.
                    startupFile = startupFiles[startupFile];
                    var xdoc = XDocument.Load(userFile);
                    var active = xdoc
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveDebugProfile")
                        .FirstOrDefault();

                    if (active != null && active.Value != startupFile)
                    {
                        active.Value = startupFile;
                        // First save to flag file so we don't cause another open 
                        // attempt via the OpenStartupFile task.
                        File.WriteAllText(flagFile, startupFile);
                        xdoc.Save(userFile);
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail($"Failed to load or update .user file: {e}");
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (rdtCookie != 0 && rdt != null)
                Try(() => rdt.UnadviseRunningDocTableEvents(rdtCookie));

            if (selectionCookie != 0 && selection != null)
                Try(() => selection.UnadviseSelectionEvents(selectionCookie));

            watcher.Dispose();
        }

        void Try(Action action)
        {
            try { action(); }
            catch (Exception e) { Debug.WriteLine(e); }
        }

        int IVsRunningDocTableEvents.OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            // The MSBuild targets should have created it in target SelectStartupFile.
            if (!File.Exists(userFile))
                return 0;

            if ((grfAttribs & (uint)__VSRDTATTRIB.RDTA_DocDataReloaded) != 0 ||
                (grfAttribs & (uint)__VSRDTATTRIB.RDTA_MkDocument) != 0)
            {
                UpdateStartupFile(((IVsRunningDocumentTable4)rdt!).GetDocumentMoniker(docCookie));
            }

            return 0;
        }

        int IVsSelectionEvents.OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            // No-op on multi-selection.
            if (pMISNew == null &&
                pHierNew != null &&
                pHierNew.GetCanonicalName(itemidNew, out var path) == 0)
            {
                UpdateStartupFile(path);
            }

            return 0;
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => 0;

        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => 0;

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie) => 0;

        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => 0;

        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => 0;

        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew) => 0;

        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive) => 0;
    }
}
