using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using System.Threading;

namespace SmallSharp
{
    static class WindowsInterop
    {
        static readonly Regex versionExpr = new Regex(@"Microsoft Visual Studio (?<version>\d\d\.\d)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static void EnsureOpened(string filePath, TimeSpan delay = default)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return;

            if (delay != default)
                Thread.Sleep(delay);

            var dte = GetDTE();
            if (dte == null)
                return;

            var maxAttempts = 5;
            var exceptions = new List<Exception>();

            for (var i = 0; i < maxAttempts; i++)
            {
                try
                {
                    dte.ExecuteCommand("File.OpenFile", filePath);
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                    Thread.Sleep(500);
                }
            }

            // NOTE: inspect exceptions variable
            Debug.Fail($"Failed to open {filePath} after 5 attempts.");
        }

        public static IServiceProvider? GetServiceProvider(TimeSpan delay = default)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                return null;

            if (delay != default)
                Thread.Sleep(delay);

            try
            {

                var dte = GetDTE();
                if (dte == null)
                    return null;

                return new OleServiceProvider(dte);
            }
            catch (Exception e)
            {
                Debug.Fail($"Failed to get IDE service provider: {e}");
                return null;
            }
        }

        static EnvDTE.DTE? GetDTE()
        {
            var window = NativeMethods.GetForegroundWindow();
            var process = Process.GetProcessesByName("devenv").FirstOrDefault(x => x.MainWindowHandle == window);

            if (process == null)
                return null;

            var devEnv = process.MainModule.FileName;
            var version = versionExpr.Match(devEnv).Groups["version"];
            if (!version.Success)
            {
                var ini = Path.ChangeExtension(devEnv, "isolation.ini");
                if (!File.Exists(ini))
                    throw new NotSupportedException("Could not determine Visual Studio version from running process from " + devEnv);

                if (!Version.TryParse(File
                        .ReadAllLines(ini)
                        .Where(line => line.StartsWith("InstallationVersion=", StringComparison.Ordinal))
                        .FirstOrDefault()?
                        .Substring(20), out var v))
                    throw new NotSupportedException("Could not determine the version of Visual Studio from devenv.isolation.ini at " + ini);

                return GetComObject<EnvDTE.DTE>(string.Format("!{0}.{1}.0:{2}",
                    "VisualStudio.DTE", v.Major, process.Id), TimeSpan.FromSeconds(2));
            }
            else
            {
                return GetComObject<EnvDTE.DTE>(string.Format("!{0}.{1}:{2}",
                    "VisualStudio.DTE", version.Value, process.Id), TimeSpan.FromSeconds(2));
            }
        }

        static T? GetComObject<T>(string monikerName, TimeSpan retryTimeout)
        {
            object? comObject;
            var stopwatch = Stopwatch.StartNew();
            do
            {
                comObject = GetComObject(monikerName);
                if (comObject != null)
                    break;

                System.Threading.Thread.Sleep(100);
            }

            while (stopwatch.Elapsed < retryTimeout);

            return (T)comObject;
        }

        static object? GetComObject(string monikerName)
        {
            object? comObject = null;
            try
            {
                IRunningObjectTable table;
                IEnumMoniker moniker;
                if (NativeMethods.Failed(NativeMethods.GetRunningObjectTable(0, out table)))
                    return null;

                table.EnumRunning(out moniker);
                moniker.Reset();
                var pceltFetched = IntPtr.Zero;
                var rgelt = new IMoniker[1];

                while (moniker.Next(1, rgelt, pceltFetched) == 0)
                {
                    IBindCtx ctx;
                    if (!NativeMethods.Failed(NativeMethods.CreateBindCtx(0, out ctx)))
                    {
                        string displayName;
                        rgelt[0].GetDisplayName(ctx, null, out displayName);
                        if (displayName == monikerName)
                        {
                            table.GetObject(rgelt[0], out comObject);
                            return comObject;
                        }
                    }
                }
            }
            catch
            {
                return null;
            }

            return comObject;
        }
    }

    internal class OleServiceProvider : IServiceProvider
    {
        readonly Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider;

        public OleServiceProvider(Microsoft.VisualStudio.OLE.Interop.IServiceProvider serviceProvider)
            => this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public OleServiceProvider(EnvDTE.DTE dte)
            : this((Microsoft.VisualStudio.OLE.Interop.IServiceProvider)dte) { }

        public object? GetService(Type serviceType)
            => GetService((serviceType ?? throw new ArgumentNullException(nameof(serviceType))).GUID);

        object? GetService(Guid guid)
        {
            if (guid == Guid.Empty)
                return null;

            if (guid == NativeMethods.IID_IServiceProvider)
                return serviceProvider;

            try
            {
                var riid = NativeMethods.IID_IUnknown;
                if (NativeMethods.Succeeded(serviceProvider.QueryService(ref guid, ref riid, out var zero)) && (IntPtr.Zero != zero))
                {
                    try
                    {
                        return Marshal.GetObjectForIUnknown(zero);
                    }
                    finally
                    {
                        Marshal.Release(zero);
                    }
                }
            }
            catch (Exception exception) when (
                exception is OutOfMemoryException ||
                exception is StackOverflowException ||
                exception is AccessViolationException ||
                exception is AppDomainUnloadedException ||
                exception is BadImageFormatException ||
                exception is DivideByZeroException)
            {
                throw;
            }

            return null;
        }
    }
}