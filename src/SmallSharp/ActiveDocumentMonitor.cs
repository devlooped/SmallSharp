using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmallSharp.Build
{
    class ActiveDocumentMonitor : MarshalByRefObject, IDisposable, IVsRunningDocTableEvents, IVsSelectionEvents, IVsSolutionEvents
    {
        IVsSolution? solution;
        IVsRunningDocumentTable? rdt;
        IVsMonitorSelection? selection;

        uint solutionCookie;
        uint rdtCookie;
        uint selectionCookie;

        string launchProfilesPath;
        string userFile;
        Dictionary<string, string> startupFiles;

        string? activeFile;

        public ActiveDocumentMonitor(string launchProfilesPath, string userFile,
            string[] startupFiles, IServiceProvider services)
        {
            this.launchProfilesPath = launchProfilesPath;
            this.userFile = userFile;
            this.startupFiles = startupFiles.ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);

            solution = (IVsSolution)services.GetService(typeof(SVsSolution));
            rdt = (IVsRunningDocumentTable)services.GetService(typeof(SVsRunningDocumentTable));
            selection = (IVsMonitorSelection)services.GetService(typeof(SVsShellMonitorSelection));

            EnsureMonitoring();
        }

        public void Refresh(string launchProfiles, string userFile, string[] startupFiles)
        {
            launchProfilesPath = launchProfiles;
            this.userFile = userFile;
            this.startupFiles = startupFiles.ToDictionary(x => x, StringComparer.OrdinalIgnoreCase);

            EnsureMonitoring();

            // For new files, we get the update before the new item is added to 
            // msbuild top-level files, so we retry on refresh
            UpdateStartupFile(activeFile);
        }

        void EnsureMonitoring()
        {
            if (solutionCookie == 0 && solution != null)
                solution.AdviseSolutionEvents(this, out solutionCookie);

            if (rdtCookie == 0 && rdt != null)
                rdt.AdviseRunningDocTableEvents(this, out rdtCookie);

            if (selectionCookie == 0 && selection != null)
                selection.AdviseSelectionEvents(this, out selectionCookie);
        }

        void UpdateStartupFile(string? path)
        {
            activeFile = path;

            if (!string.IsNullOrEmpty(path) &&
                path!.IndexOfAny(Path.GetInvalidPathChars()) == -1 &&
                startupFiles.TryGetValue(Path.GetFileName(path), out var startupFile))
            {
                var settings = new JObject(
                    new JProperty("profiles", new JObject(
                        new JProperty(startupFile, new JObject(
                            new JProperty("commandName", "Project")
                        ))
                    ))
                );

                var json = settings.ToString(Formatting.Indented);

                // Only write if different content.
                if (File.Exists(launchProfilesPath) &&
                    File.ReadAllText(launchProfilesPath) == json)
                    return;

                File.WriteAllText(launchProfilesPath, json);

                try
                {
                    // Get the value as it was exists in the original dictionary, 
                    // since it has to match what the source generator created in the 
                    // launch profiles.
                    var xdoc = XDocument.Load(userFile);
                    var active = xdoc
                        .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveDebugProfile")
                        .FirstOrDefault();

                    if (active != null && !startupFile.Equals(active.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        active.Value = startupFile;
                        xdoc.Save(userFile);
                    }
                }
                catch (Exception e)
                {
                    Debug.Fail($"Failed to load or update .user file: {e}");
                }
            }
        }

        public void Dispose()
        {
            if (solutionCookie != 0 && solution != null)
                Try(() => solution.UnadviseSolutionEvents(solutionCookie));

            solutionCookie = 0;

            if (rdtCookie != 0 && rdt != null)
                Try(() => rdt.UnadviseRunningDocTableEvents(rdtCookie));

            rdtCookie = 0;

            if (selectionCookie != 0 && selection != null)
                Try(() => selection.UnadviseSelectionEvents(selectionCookie));

            selectionCookie = 0;
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

        int IVsSolutionEvents.OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            Dispose();
            return 0;
        }

        int IVsSolutionEvents.OnBeforeCloseSolution(object pUnkReserved)
        {
            Dispose();
            return 0;
        }

        int IVsRunningDocTableEvents.OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => 0;
        int IVsRunningDocTableEvents.OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining) => 0;
        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie) => 0;
        int IVsRunningDocTableEvents.OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => 0;
        int IVsRunningDocTableEvents.OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => 0;
        int IVsSelectionEvents.OnElementValueChanged(uint elementid, object varValueOld, object varValueNew) => 0;
        int IVsSelectionEvents.OnCmdUIContextChanged(uint dwCmdUICookie, int fActive) => 0;
        int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => throw new NotImplementedException();
        int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => throw new NotImplementedException();
        int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => throw new NotImplementedException();
        int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => throw new NotImplementedException();
        int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => throw new NotImplementedException();
        int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => throw new NotImplementedException();
        int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => throw new NotImplementedException();
        int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved) => throw new NotImplementedException();
    }
}
