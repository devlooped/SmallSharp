using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json.Linq;

namespace SmallSharp
{
    public class JsonPeek : Task
    {
        public void Test()
        {
            JsonInputPath = @"C:\Code\kzu\script\Properties\launchSettings.json";
            JPath = "profiles";
            Execute();
        }

        [Required]
        public string JsonInputPath { get; set; } = "";

        [Required]
        public string JPath { get; set; } = "";

        [Output]
        public ITaskItem[] Result { get; private set; } = Array.Empty<ITaskItem>();

        public override bool Execute()
        {
            var root = JToken.Parse(File.ReadAllText(JsonInputPath));
            Result = root.SelectTokens(JPath, false)
                .SelectMany(json => GetTaskItems(json))
                .ToArray();

            return true;
        }

        static IEnumerable<ITaskItem> GetTaskItems(JToken json) => json switch
        {
            JArray jarr => jarr.SelectMany(x => GetTaskItems(x)),
            JObject jobj => new[]
            {
                new TaskItem(jobj.ToString(), jobj.Properties()
                    .Select(prop => new { prop.Name, Value = prop.Value.ToString() } )
                    .ToDictionary(x => x.Name, x => x.Value))
            },
            _ => new[] { new TaskItem(json.ToString(), new Dictionary<string, string>
            {
                { "Value", json.ToString() }
            })}
        };
    }
}
