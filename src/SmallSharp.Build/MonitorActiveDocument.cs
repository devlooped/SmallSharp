using System;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SmallSharp.Build
{
    public class MonitorActiveDocument : Task
    {
        [Required]
        public string? FlagFile { get; set; }

        [Required]
        public string? LaunchProfiles { get; set; }

        [Required]
        public string? UserFile { get; set; }

        public override bool Execute()
        {
            if (LaunchProfiles == null || UserFile == null || FlagFile == null)
                return true;

            try
            {
                if (BuildEngine4.GetRegisteredTaskObject(nameof(ActiveDocumentMonitor), RegisteredTaskObjectLifetime.AppDomain) is not ActiveDocumentMonitor monitor)
                {
                    if (WindowsInterop.GetServiceProvider() is IServiceProvider services)
                    {
                        BuildEngine4.RegisterTaskObject(nameof(ActiveDocumentMonitor),
                            new ActiveDocumentMonitor(LaunchProfiles, UserFile, FlagFile, services),
                            RegisteredTaskObjectLifetime.AppDomain, false);
                    }
                }
                else
                {
                    // NOTE: this means we only support ONE project/launchProfiles per IDE.
                    monitor.Refresh(LaunchProfiles, UserFile, FlagFile);
                }
            }
            catch (Exception e)
            {
                Log.LogWarning($"Failed to start active document monitoring: {e}");
            }

            return true;
        }
    }
}
