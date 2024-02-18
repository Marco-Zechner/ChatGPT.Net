using ChatGPT.Net;

namespace ChatGPT.NetTest
{
    internal class Program
    {
        const string KEY = "APIKEY";

        static float totalCost = 0;
        static float lastCost = 0;
        static GptModelInfo model;

        static async Task Main(string[] args)
        {
            model = GptModels.GetModel("gpt-3.5-turbo-0125");
            
            PrintBaseInfo();


            List<GptMessage> messages = [];

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("User: ");
                Console.ForegroundColor = ConsoleColor.White;
                messages.Add(new GptMessage(GptRole.user, Console.ReadLine()));

                //Console.WriteLine(messages[0].Content);

                GptResponse response = await GptApi.SendMessageAsync(KEY, model.Name, messages);

                if (response is GptErrorResponse)
                {
                    Console.WriteLine("Error: " + (response as GptErrorResponse).Error.Message);
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("GPT:  ");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine(response.Choices[0].Message.Content);
                Console.BackgroundColor = ConsoleColor.Black;

                lastCost = (response.Usage.PromptTokens * model.PromptCostPer1kToken + response.Usage.CompletionTokens * model.CompletionCostPer1kToken) / 1000;
                totalCost += lastCost;

                PrintBaseInfo();
                Console.WriteLine();
            }
        }

        private static void PrintBaseInfo()
        {
            int currentTop = Math.Max(Console.CursorTop, 4);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Model:         {model.Name}");
            Console.WriteLine($"TotalCost:     {totalCost:N6}$");
            Console.WriteLine($"LastPromtCost: {lastCost:N6}$");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('-', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, currentTop);
        }
    }
}
