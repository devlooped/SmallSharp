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
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new GlobalStatementReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            context.CheckDebugger();

            var shouldRun = context.AnalyzerConfigOptions.GlobalOptions
                .TryGetValue("build_property.SkipCompilerExecution", out var skipValue) &&
                bool.TryParse(skipValue, out var skipCompiler) && skipCompiler;

            if (!shouldRun)
                return;

            var documents = ((GlobalStatementReceiver)context.SyntaxReceiver).Documents;

            var settings = new JObject(
                new JProperty("profiles", new JObject(
                    documents.Select(path => new JProperty(Path.GetFileName(path), new JObject(
                        new JProperty("commandName", "Project"), 
                        new JProperty("environmentVariables", new JObject(
                            new JProperty("source", Path.GetFileName(path))
                        ))
                    )))
                ))
            );

            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var directory))
            {
                File.WriteAllText(
                    Path.Combine(directory, "Properties", "launchSettings.json"),
                    settings.ToString(Formatting.Indented));
            }
        }

        class GlobalStatementReceiver : ISyntaxReceiver
        {
            public HashSet<string> Documents { get; } = new HashSet<string>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GlobalStatement))
                    Documents.Add(syntaxNode.SyntaxTree.FilePath);
            }
        }
    }
}
