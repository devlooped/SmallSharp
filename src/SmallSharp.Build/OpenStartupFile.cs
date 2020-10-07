using System;
using System.IO;
using EnvDTE;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SmallSharp.Build
{
    public class OpenStartupFile : Task
    {
        [Required]
        public string? FlagFile { get; set; }
     
        public string? StartupFile { get; set; }

        public override bool Execute()
        {
            if (FlagFile == null || StartupFile == null)
                return true;

            if (!File.Exists(FlagFile) ||
                File.ReadAllText(FlagFile) != StartupFile)
            {
                // This defers the opening until the build completes.
                BuildEngine4.RegisterTaskObject(
                    StartupFile,
                    new DisposableAction(() => WindowsInterop.EnsureOpened(StartupFile)),
                    RegisteredTaskObjectLifetime.Build, false);

                File.WriteAllText(FlagFile, StartupFile);
            }

            return true;
        }
    }
}
