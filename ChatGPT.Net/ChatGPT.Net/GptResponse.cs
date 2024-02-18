using Newtonsoft.Json;

namespace ChatGPT.Net
{
    
    public class GptErrorResponse : GptResponse
    {
        [JsonProperty("error")]
        public GptError? Error { get; set; }
    }

    public class GptResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("choices")]
        public List<GptChoice> Choices { get; set; }

        [JsonProperty("usage")]
        public GptUsage Usage { get; set; }

        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; set; }

    }

    public class GptChoice
    {
        [JsonProperty("index")]
        public long Index { get; set; }

        [JsonProperty("message")]
        public GptFunctionCalls Message { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }

    public class GptUsage
    {
        [JsonProperty("prompt_tokens")]
        public long PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public long CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public long TotalTokens { get; set; }
    }

    public class GptError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("param")]
        public object Param { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
