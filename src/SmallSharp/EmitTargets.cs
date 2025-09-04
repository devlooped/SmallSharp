using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static SmallSharp.TaskItemFactory;

namespace SmallSharp;

public class EmitTargets : Task
{
    static readonly Regex sdkExpr = new(@"^#:sdk\s+([^@]+?)(@(.+))?$");
    static readonly Regex packageExpr = new(@"^#:package\s+([^@]+)@(.+)$");
    static readonly Regex propertyExpr = new(@"^#:property\s+([^\s]+)\s+(.+)$");

    [Required]
    public required ITaskItem StartupFile { get; set; }

    [Required]
    public required string BaseIntermediateOutputPath { get; set; }

    [Required]
    public required string PropsFile { get; set; }

    [Required]
    public required string TargetsFile { get; set; }

    [Required]
    public required bool UsingSDK { get; set; }

    public ITaskItem[] PackageReferences { get; set; } = [];

    [Output]
    public ITaskItem[] Packages { get; set; } = [];

    [Output]
    public ITaskItem[] Sdks { get; set; } = [];

    [Output]
    public ITaskItem[] Properties { get; set; } = [];

    [Output]
    public bool RestoreNeeded { get; set; } = false;

    public override bool Execute()
    {
        if (StartupFile is null)
            return false;

        var packages = new List<ITaskItem>();
        var sdkItems = new List<ITaskItem>();
        var propItems = new List<ITaskItem>();

        var filePath = StartupFile.GetMetadata("FullPath");
        var contents = File.ReadAllLines(filePath);

        var items = new List<XElement>();
        var properties = new List<XElement>();
        var sdks = new List<XAttribute[]>();

        foreach (var line in contents)
        {
            if (packageExpr.Match(line) is { Success: true } match)
            {
                var id = match.Groups[1].Value.Trim();
                var version = match.Groups[2].Value.Trim();

                packages.Add(NewTaskItem(id, [("Version", version)]));

                items.Add(new XElement("PackageReference",
                    new XAttribute("Include", id),
                    new XAttribute("Version", version)));
            }
            else if (sdkExpr.Match(line) is { Success: true } sdkMatch)
            {
                var name = sdkMatch.Groups[1].Value.Trim();
                var version = sdkMatch.Groups[2].Value.Trim();
                if (!string.IsNullOrEmpty(version))
                {
                    sdkItems.Add(NewTaskItem(name, [("Version", version)]));
                    sdks.Add([new XAttribute("Sdk", name), new XAttribute("Version", version)]);
                }
                else
                {
                    sdkItems.Add(new TaskItem(name));
                    sdks.Add([new XAttribute("Sdk", name)]);
                }
            }
            else if (propertyExpr.Match(line) is { Success: true } propMatch)
            {
                var name = propMatch.Groups[1].Value.Trim();
                var value = propMatch.Groups[2].Value.Trim();

                propItems.Add(NewTaskItem(name, [("Value", value)]));
                properties.Add(new XElement(name, value));
            }
        }

        Packages = [.. packages];
        Sdks = [.. sdkItems];
        Properties = [.. propItems];

        // We only emit the default SDK if the SmallSharpSDK is in use, since otherwise the 
        // project file is expected to define its own SDK and we'd be duplicating it.
        if (sdks.Count == 0)
            sdks.Add([new XAttribute("Sdk", "Microsoft.NET.Sdk")]);

        WriteXml(TargetsFile, new XElement("Project",
            new XElement("PropertyGroup", properties),
            // don't emit package references in SDK mode, since we'll add them from the SDK targets.
            UsingSDK ? new XElement("ItemGroup") : new XElement("ItemGroup", items)
        ));

        WriteXml(Path.Combine(BaseIntermediateOutputPath, "SmallSharp.sdk.props"), new XElement("Project",
            sdks.Select(x => new XElement("Import", [new XAttribute("Project", "Sdk.props"), .. x]))));

        WriteXml(Path.Combine(BaseIntermediateOutputPath, "SmallSharp.sdk.targets"), new XElement("Project",
            sdks.Select(x => new XElement("Import", [new XAttribute("Project", "Sdk.targets"), .. x]))));

        WriteXml(PropsFile, new XElement("Project",
            new XElement("PropertyGroup",
                [new XElement("SmallSharpProjectExtensionPropsImported", "true")])));

        // Determine if a restore is needed: if any discovered #:package (id+version) is not already
        // present in the incoming PackageReferences list.
        foreach (var pkg in packages)
        {
            var id = pkg.ItemSpec;
            var version = pkg.GetMetadata("Version");
            var exists = PackageReferences?.Any(r =>
                string.Equals(r.ItemSpec, id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.GetMetadata("Version"), version, StringComparison.OrdinalIgnoreCase)) == true;

            if (!exists)
            {
                RestoreNeeded = true;
                break;
            }
        }

        return true;
    }

    void WriteXml(string path, XElement root)
    {
        if (Path.GetDirectoryName(path) is { } dir)
            Directory.CreateDirectory(dir);

        using var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true });
        root.Save(writer);
    }
}
