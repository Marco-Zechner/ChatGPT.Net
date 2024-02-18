using Newtonsoft.Json;

namespace ChatGPT.Net
{
    public class GptTool
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("function")]
        public GpToolsFunction Function { get; set; } = new();
    }
    public class GpToolsFunction
    {
        [JsonProperty("name")]
        public string Name { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("parameters")]
        public GptToolsParameters Parameters { get; set; } = new();
    }

    public class GptToolsParameters
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("properties")]
        public Dictionary<string, GptPropertyDetails> Properties { get; set; } = [];

        [JsonProperty("required")]
        public List<string> Required { get; set; } = [];
    }

    public class GptPropertyDetails
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "";

        [JsonProperty("description")]
        public string Description { get; set; } = "";

        [JsonProperty("enum")]
        public List<string> Enum { get; set; } = [];
    }
}
