using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SmallSharp;

public class EmitPackages : Task
{
    static readonly Regex regex = new(@"^#:package\s+([^@]+)@(.+)$");

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

        var elements = new List<XElement>();

        foreach (var line in contents)
        {
            if (regex.Match(line) is { Success: true } match)
            {
                var id = match.Groups[1].Value.Trim();
                var version = match.Groups[2].Value.Trim();

                packages.Add(new TaskItem(id, new Dictionary<string, string>
                {
                    { "Version", version }
                }));

                elements.Add(new XElement("PackageReference",
                    new XAttribute("Include", id),
                    new XAttribute("Version", version)));

                // TODO: emit the PackageVersion element too?
                //elements.Add(new XElement("PackageVersion",
                //    new XAttribute("Include", id),
                //    new XAttribute("Version", version)));
            }
        }

        Packages = [.. packages];

        var doc = new XDocument(
            new XElement("Project",
                new XElement("ItemGroup", elements)
            )
        );

        using var writer = XmlWriter.Create(TargetsFile, new XmlWriterSettings { Indent = true });
        doc.Save(writer);

        return true;
    }
}
