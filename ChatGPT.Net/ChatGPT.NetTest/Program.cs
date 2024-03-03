using ChatGPT.Net;

namespace ChatGPT.NetTest
{
    internal class Program
    {

        static async Task Main()
        {
            bool enableTools = true;

            await CustomMessage_DEMO.CustomConversation(APIKey.KEY, enableTools, new ConsoleDebugger());

            //GptModelInfo model = GptModels.GetModel("gpt-3.5-turbo-0125");
            //GptToolConfiguration toolConfig = new();
            //GptMessage systemInstruction = new(GptRole.system, "System Testing");

            //GptChatClient humorCore = new(APIKey.KEY, model, toolConfig, systemInstruction, new ConsoleDebugger());

            //while (true)
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    Console.Write("User:     ");
            //    Console.ForegroundColor = ConsoleColor.White;
            //    var response = await humorCore.AskModelAsync(Console.ReadLine(), useDebug: true);

            //    Console.ForegroundColor = ConsoleColor.Green;
            //    Console.Write("GPT:      ");
            //    Console.ForegroundColor = ConsoleColor.White;
            //    Console.WriteLine(response.Choices[0].Message.Content);
            //}
        }
    }
}
