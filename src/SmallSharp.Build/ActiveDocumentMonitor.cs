using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;

namespace SmallSharp.Build
{
    class ActiveDocumentMonitor : MarshalByRefObject, IDisposable, IVsRunningDocTableEvents, IVsSelectionEvents
    {
        FileSystemWatcher watcher;
        readonly IVsRunningDocumentTable? rdt;
        readonly IVsMonitorSelection? selection;
        readonly uint rdtCookie;
        readonly uint selectionCookie;

        string launchProfilesPath;
        string userFile;
        string flagFile;
        Dictionary<string, string> startupFiles = new();

        public ActiveDocumentMonitor(string launchProfilesPath, string userFile, string flagFile, IServiceProvider services)
        {
            this.launchProfilesPath = launchProfilesPath;
            this.userFile = userFile;
            this.flagFile = flagFile;

            watcher = new FileSystemWatcher(Path.GetDirectoryName(launchProfilesPath))
            {
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "launchSettings.json",
            };

            watcher.Changed += (_, _) => ReloadProfiles();
            watcher.Created += (_, _) => ReloadProfiles();
            watcher.EnableRaisingEvents = true;
            ReloadProfiles();

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

            try
            {
                var json = JObject.Parse(File.ReadAllText(launchProfilesPath));
                if (json.Property("profiles") is not JProperty prop || 
                    prop.Value is not JObject profiles)
                    return;

                startupFiles = profiles.Properties().Select(p => p.Name)
                    .ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                Debug.Fail("Could not read launchSettings.json");
            }
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
                catch
                {
                    Debug.Fail("Failed to load or update .user file.");
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (rdtCookie != 0 && rdt != null)
                rdt.UnadviseRunningDocTableEvents(rdtCookie);

            if (selectionCookie != 0 && selection != null)
                selection.UnadviseSelectionEvents(selectionCookie);

            watcher.Dispose();
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
