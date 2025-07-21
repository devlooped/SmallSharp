using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SmallSharp;

public class EmitTargets : Task
{
    static readonly Regex packageExpr = new(@"^#:package\s+([^@]+)@(.+)$");
    static readonly Regex propertyExpr = new(@"^#:property\s+([^\s]+)\s+(.+)$");

    [Required]
    public ITaskItem? StartupFile { get; set; }

    [Required]
    public string TargetsFile { get; set; } = "SmallSharp.targets";

    [Output]
    public ITaskItem[] Packages { get; set; } = [];

    public override bool Execute()
    {
        if (StartupFile is null)
            return false;

        var packages = new List<ITaskItem>();
        var filePath = StartupFile.GetMetadata("FullPath");
        var contents = File.ReadAllLines(filePath);

        var items = new List<XElement>();
        var properties = new List<XElement>();

        foreach (var line in contents)
        {
            if (packageExpr.Match(line) is { Success: true } match)
            {
                var id = match.Groups[1].Value.Trim();
                var version = match.Groups[2].Value.Trim();

                packages.Add(new TaskItem(id, new Dictionary<string, string>
                {
                    { "Version", version }
                }));

                items.Add(new XElement("PackageReference",
                    new XAttribute("Include", id),
                    new XAttribute("Version", version)));
            }
            else if (propertyExpr.Match(line) is { Success: true } propMatch)
            {
                var name = propMatch.Groups[1].Value.Trim();
                var value = propMatch.Groups[2].Value.Trim();

                properties.Add(new XElement(name, value));
            }
        }

        Packages = [.. packages];

        var doc = new XDocument(
            new XElement("Project",
                new XElement("PropertyGroup", properties),
                new XElement("ItemGroup", items)
            )
        );

        using var writer = XmlWriter.Create(TargetsFile, new XmlWriterSettings { Indent = true });
        doc.Save(writer);

        return true;
    }
}
