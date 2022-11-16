using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json.Linq;

namespace SmallSharp;

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

#if DEBUG && !CI
        //Debugger.Launch();
#endif

        if (!string.IsNullOrEmpty(path) &&
            path!.IndexOfAny(Path.GetInvalidPathChars()) == -1 &&
            startupFiles.TryGetValue(Path.GetFileName(path), out var startupFile))
        {
            // NOTE: we could skip writing the profiles altogether, since the 
            // targets already JsonPoke these entries. This causes issues, however, 
            // when the entries there don't match *exactly* what the compile files 
            // are (i.e. WSL). In this scenario, the project system remains in an 
            // in-memory "dirty" state where it doesn't refresh the active debug 
            // profile anymore because it keeps its own WSL selection around.
            var settings = new JObject(
                new JProperty("profiles", new JObject(
                    startupFiles.Select(file => new JProperty(file.Key, new JObject(
                        new JProperty("commandName", "Project")
                    )))
                ))
            );

            var json = settings.ToString(Newtonsoft.Json.Formatting.Indented);

            // Only write if different content.
            if (File.Exists(launchProfilesPath) &&
                File.ReadAllText(launchProfilesPath) != json)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(launchProfilesPath));
                File.WriteAllText(launchProfilesPath, json);
            }

            try
            {
                // Get the value as it was exists in the original dictionary, 
                // since it has to match what the source generator created in the 
                // launch profiles.
                var xdoc = XDocument.Load(userFile);
                var save = false;

                // The additional ActiveCompile is a prerequisite for supporting non-file 
                // debug profiles, such as WSL. At this point, it's not working, but it's 
                // will be based on this extra property eventually, so we keep it here.

                var activeCompile = xdoc
                    .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveCompile")
                    .FirstOrDefault();

                if (activeCompile == null)
                {
                    var props = xdoc.Root.Elements("{http://schemas.microsoft.com/developer/msbuild/2003}PropertyGroup").LastOrDefault();
                    if (props == null)
                    {
                        props = new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}PropertyGroup");
                        xdoc.Root.Add(props);
                    }
                    activeCompile = new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveCompile", startupFile);
                    props.Add(activeCompile);
                    save = true;
                }

                if (!startupFile.Equals(activeCompile.Value, StringComparison.OrdinalIgnoreCase))
                {
                    activeCompile.Value = startupFile;
                    save = true;
                }

                var activeDebug = xdoc
                    .Descendants("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveDebugProfile")
                    .FirstOrDefault();

                if (activeDebug == null)
                {
                    var props = xdoc.Root.Elements("{http://schemas.microsoft.com/developer/msbuild/2003}PropertyGroup").LastOrDefault();
                    if (props == null)
                    {
                        props = new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}PropertyGroup");
                        xdoc.Root.Add(props);
                    }
                    activeDebug = new XElement("{http://schemas.microsoft.com/developer/msbuild/2003}ActiveDebugProfile", startupFile);
                    props.Add(activeDebug);
                    save = true;
                }

                if (activeDebug.Value != null &&
                    activeDebug.Value.IndexOfAny(Path.GetInvalidPathChars()) == -1 &&
                    // TODO: Don't mess with debug profile unless it's a file-like name, so we can support WSL
                    // Path.HasExtension(activeDebug.Value) && 
                    !startupFile.Equals(activeDebug.Value, StringComparison.OrdinalIgnoreCase))
                {
                    activeDebug.Value = startupFile;
                    save = true;
                }

                if (save)
                {
                    xdoc.Save(userFile);
                    File.SetLastWriteTime(launchProfilesPath, DateTime.Now);
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
        solution = null;
        rdt = null;
        selection = null;
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
    int IVsSolutionEvents.OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) => 0;
    int IVsSolutionEvents.OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) => 0;
    int IVsSolutionEvents.OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) => 0;
    int IVsSolutionEvents.OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) => 0;
    int IVsSolutionEvents.OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) => 0;
    int IVsSolutionEvents.OnAfterOpenSolution(object pUnkReserved, int fNewSolution) => 0;
    int IVsSolutionEvents.OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) => 0;
    int IVsSolutionEvents.OnAfterCloseSolution(object pUnkReserved) => 0;
}
