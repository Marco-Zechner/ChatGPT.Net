using static ChatGPT.Net.GptApiAttribute;

namespace ChatGPT.NetTest
{
    internal class Program
    {

        static async Task Main()
        {
            bool enableTools = true;
            bool debug = true;

            await CustomMessage_DEMO.CustomConversation(APIKey.KEY, enableTools, debug);
        }
    }
}
