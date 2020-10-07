using Microsoft.CodeAnalysis.Diagnostics;

namespace SmallSharp
{
    static class AnalyzerConfigOptionsExtensions
    {
        public static bool IsEnabled(this AnalyzerOptions options, string optionName) 
            => IsEnabled(options.AnalyzerConfigOptionsProvider.GlobalOptions, optionName);

        public static bool IsEnabled(this AnalyzerConfigOptionsProvider options, string optionName)
            => IsEnabled(options.GlobalOptions, optionName);

        public static bool IsEnabled(this AnalyzerConfigOptions options, string optionName)
            => options.TryGetValue("build_property." + optionName, out var value) && bool.TryParse(value, out var enabled) && enabled;
    }
}
