namespace ChatGPT.Net
{
    public class GptToolConfiguration
    {
        public GptToolConfiguration() => EnableAll();

        public HashSet<string> EnabledToolNames { private set; get; } = [];

        public List<GptTool> GetEnabledTools() => GptToolHandler.Tools.Where(tool => EnabledToolNames.Contains(tool.Function.Name)).ToList();
        public void EnableAll() => EnabledToolNames.UnionWith(GptToolHandler.Tools.Select(t => t.Function.Name));
        public void DisableAll() => EnabledToolNames.Clear();


        public string EnableTool(string toolName)
        {
            if (GptToolHandler.Tools.Any(t => t.Function.Name == toolName))
            {
                EnabledToolNames.Add(toolName);
                return $"{toolName} enabled.";
            }
            return $"Tool {toolName} not found.";
        }

        public string DisableTool(string toolName)
        {
            if (GptToolHandler.Tools.Any(t => t.Function.Name == toolName))
            {
                EnabledToolNames.Remove(toolName);
                return $"{toolName} diabled.";
            }
            return $"Tool {toolName} not found.";
        }


        public string EnableToolClass(Type toolClass)
        {
            if (GptToolHandler.ClassToMethodNames.TryGetValue(toolClass, out var methodNames))
            {
                foreach (var name in methodNames)
                {
                    EnableTool(name);
                }
            }
            return $"Class {toolClass} not found.";

        }

        public string DisableToolClass(Type toolClass)
        {
            if (GptToolHandler.ClassToMethodNames.TryGetValue(toolClass, out var methodNames))
            {
                foreach (var name in methodNames)
                {
                    DisableTool(name);
                }
            }
            return $"Class {toolClass} not found.";
        }
    }
}
