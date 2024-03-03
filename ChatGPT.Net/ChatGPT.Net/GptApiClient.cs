using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ChatGPT.Net
{
    public class GptApiClient(string apiKey, GptModelInfo model, IGptDebug debug = null)
    {
        private readonly string apiKey = apiKey;
        private readonly GptModelInfo model = model;

        public async Task<GptResponse> SingleCompletionAsync(IEnumerable<GptMessage> messages, List<GptTool>? tools = null, bool useDebug = false)
        {
            string jsonPrompt = GenerateJsonRequest(model, messages, tools);

            if (debug != null && useDebug)
                debug.Debug(jsonPrompt, 255, 165, 0);

            (string jsonResponse, bool success) = await SendJsonRequestAsync(apiKey, jsonPrompt);

            if (!success)
            {
                if (debug == null || !useDebug)
                {
                    throw new Exception(jsonResponse);
                }

                debug.Debug(jsonResponse, 255, 0, 0);
            }
            else if (debug != null && useDebug)
                debug.Debug(jsonResponse, 0, 100, 0);

            return GenerateResponseFromJson(jsonResponse, success);
        }

        public async Task<GptResponse> ConversationCompletionAsync(string conversationID, List<GptTool>? tools = null)
        {
            var messages = GptConversations.GetMessages(conversationID);

            GptResponse response = await SingleCompletionAsync(messages, tools);

            GptConversations.AddMessage(conversationID, new GptMessage(GptRole.assistant, response.Choices[0].Message.Content));

            return response;
        }

        public static async Task<(string json, bool success)> SendJsonRequestAsync(string apiKey, string jsonPrompt)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.openai.com/v1/chat/completions"),
                Headers =
                {
                    {"Authorization", $"Bearer {apiKey}" }
                },
                Content = new StringContent(jsonPrompt)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            return (await response.Content.ReadAsStringAsync(), response.IsSuccessStatusCode);
        }

        public static string GenerateJsonRequest(GptModelInfo model, IEnumerable<GptMessage> messages, List<GptTool>? tools = null)
        {
            var requestBody = new GptRequest
            {
                Messages = messages.ToList(),
                Model = model.Name,
                Tools = tools,
            };

            return JsonConvert.SerializeObject(requestBody, Formatting.Indented);
        }

        public static GptResponse GenerateResponseFromJson(string jsonResponse, bool success)
        {
            if (!success)
            {
                return JsonConvert.DeserializeObject<GptErrorResponse>(jsonResponse) ?? throw new Exception("Unknown error");
            }

            return JsonConvert.DeserializeObject<GptResponse>(jsonResponse) ?? throw new Exception("Unknown error");
        }
    }
}
