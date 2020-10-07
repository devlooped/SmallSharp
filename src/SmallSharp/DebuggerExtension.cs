using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SmallSharp
{
    static class DebuggerExtension
    {
        //[Conditional("DEBUG")]
        public static void CheckDebugger(this GeneratorExecutionContext context, string generatorName = nameof(SmallSharp))
            => context.AnalyzerConfigOptions.CheckDebugger(generatorName);

        public static void CheckDebugger(this AnalyzerConfigOptionsProvider provider, string generatorName = nameof(SmallSharp))
        {
            if (Process.GetCurrentProcess().ProcessName == "devenv")
                return;

            if (provider.IsEnabled("DebugSourceGenerators") ||
                provider.IsEnabled("Debug" + generatorName))
                Debugger.Launch();
        }
    }
}
