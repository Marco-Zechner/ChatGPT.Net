using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ChatGPT.Net
{
    public static class GptApi
    {
        public static async Task<GptResponse> SendMessageAsync(string apiKey, string model, List<GptMessage> messages, List<GptTool>? tools = null)
        {
            var requestBody = new GptRequest
            {
                Messages = messages,
                Model = model,
                Tools = tools
            };

            string json = JsonConvert.SerializeObject(requestBody);

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"https://api.openai.com/v1/chat/completions"),
                Headers =
                {
                    {"Authorization", $"Bearer {apiKey}" }
                },
                Content = new StringContent(json)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            GptResponse content;

            //Console.ForegroundColor = ConsoleColor.DarkYellow;
            //Console.WriteLine(jsonResponse);
            //Console.ForegroundColor = ConsoleColor.White;


            if (!response.IsSuccessStatusCode)
            {
                content = JsonConvert.DeserializeObject<GptErrorResponse>(jsonResponse) ?? throw new Exception("Unknown error");
                return content;
            }

            //response.EnsureSuccessStatusCode();

            content = JsonConvert.DeserializeObject<GptResponse>(jsonResponse) ?? throw new Exception("Unknown error");

            return content;
        }
    }
}
