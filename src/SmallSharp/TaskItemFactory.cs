using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Utilities;

namespace SmallSharp;

static class TaskItemFactory
{
    public static TaskItem NewTaskItem(string itemSpec, Dictionary<string, string> metadata) => new(itemSpec, metadata);

    public static TaskItem NewTaskItem(string itemSpec, params (string Key, string Value)[] metadata)
        => new(itemSpec, metadata.ToDictionary(x => x.Key, x => x.Value));
}
