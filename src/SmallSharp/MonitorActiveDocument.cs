using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SmallSharp.Build
{
    public class MonitorActiveDocument : Task
    {
        [Required]
        public string? LaunchProfiles { get; set; }

        [Required]
        public string? UserFile { get; set; }

        [Required]
        public ITaskItem[] StartupFiles { get; set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(LaunchProfiles) ||
                string.IsNullOrEmpty(UserFile))
            {
                Debug.Fail("Should have gotten something to monitor");
                return true;
            }

            try
            {
                if (BuildEngine4.GetRegisteredTaskObject(nameof(ActiveDocumentMonitor), RegisteredTaskObjectLifetime.AppDomain) is not ActiveDocumentMonitor monitor)
                {
                    var maxAttempts = 5;
                    for (var i = 0; i < maxAttempts; i++)
                    {
                        if (WindowsInterop.GetServiceProvider() is IServiceProvider services)
                        {
                            BuildEngine4.RegisterTaskObject(nameof(ActiveDocumentMonitor),
                                new ActiveDocumentMonitor(LaunchProfiles!, UserFile!,
                                    StartupFiles.Select(x => x.ItemSpec).ToArray(), services),
                                RegisteredTaskObjectLifetime.AppDomain, false);

                            return true;
                        }
                        else
                        {
                            try
                            {
                                BuildEngine4.Yield();
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                            }
                            Thread.Sleep(200);
                        }
                    }

                    Debug.Fail("Failed to get IServiceProvider to monitor for active document.");
                }
                else
                {
                    // NOTE: this means we only support ONE project/launchProfiles per IDE.
                    monitor.Refresh(LaunchProfiles!, UserFile!,
                        StartupFiles.Select(x => x.ItemSpec).ToArray());
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
