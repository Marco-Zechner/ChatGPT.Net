using Newtonsoft.Json;

namespace ChatGPT.Net
{
    public class GptMessage(GptRole role, string? content)
    {
        [JsonProperty("role")]
        public string Role { get; set; } = role.ToString();

        [JsonProperty("content")]
        public string? Content { get; set; } = content;
    }

    public class GptFunctionReturn(GptRole role, string content, string toolCallID, string name) : GptMessage(role, content)
    {
        [JsonProperty("tool_call_id")]
        public string ToolCallID { get; set; } = toolCallID;

        [JsonProperty("name")]
        public string Name { get; set; } = name;
    }

    public class GptFunctionCalls(List<GptToolCall> toolCalls) : GptMessage(GptRole.assistant, null)
    {
        [JsonProperty("tool_calls", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<GptToolCall> ToolCalls { get; set; } = toolCalls;
    }

    public class GptToolCall
    {
        [JsonProperty("id")]
        public string ID { get; set; } = "";

        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("function")]
        public GptFunctionCall Function { get; set; } = new();
    }

    public class GptFunctionCall
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";
        [JsonProperty("arguments")]
        public string Arguments { get; set; } = "";
    }
}
