using Newtonsoft.Json;

namespace ChatGPT.Net
{
    public class GptRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "gpt-3.5-turbo-0125";

        [JsonProperty("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonProperty("max_tokens")]
        public long MaxTokens { get; set; } = 500;

        [JsonProperty("n")]
        private long N { get; set; } = 1;

        [JsonProperty("stop")]
        public string[]? Stop { get; set; }

        [JsonProperty("top_p")]
        public double TopP { get; set; } = 0.9;
        [JsonProperty("presence_penalty")]
        public double PresencePenalty { get; set; } = 0.4;

        [JsonProperty("frequency_penalty")]
        public double FrequencyPenalty { get; set; } = 0.0;

        [JsonProperty("messages")]
        public List<GptMessage> Messages { get; set; } = [];

        [JsonProperty("tools", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private List<GptTool> tools = [];

        [JsonIgnore]
        public List<GptTool> Tools
        {
            get => tools;
            set 
            {
                if (value == null || value.Count == 0)
                {
                    tools = null;
                    ToolChoice = null;
                }
                else
                {
                    tools = value;
                    if (ToolChoice == null)
                    {
                        ToolChoice = "auto";
                    }
                }
            }
        }

        [JsonProperty("tool_choice", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ToolChoice { get; set; } = "auto";
    }
}
