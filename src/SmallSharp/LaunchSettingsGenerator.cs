using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;

namespace SmallSharp
{
    [Generator]
    public class LaunchSettingsGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            context.CheckDebugger();

            var documents = from additional in context.AdditionalFiles
                            let options = context.AnalyzerConfigOptions.GetOptions(additional)
                            let compile = options.TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var itemType) && itemType == "Compile"
                            where compile
                            select additional.Path;

            var settings = new JObject(
                new JProperty("profiles", new JObject(
                    documents.OrderBy(path => Path.GetFileName(path)).Select(path => new JProperty(Path.GetFileName(path), new JObject(
                        new JProperty("commandName", "Project")
                    )))
                ))
            );

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var directory))
            {
                Directory.CreateDirectory(Path.Combine(directory, "Properties"));
                File.WriteAllText(
                    Path.Combine(directory, "Properties", "launchSettings.json"),
                    settings.ToString(Formatting.Indented));
            }
        }
    }
}
