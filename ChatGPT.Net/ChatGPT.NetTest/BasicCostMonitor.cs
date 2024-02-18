using ChatGPT.Net;

namespace ChatGPT.NetTest
{
    public class BasicCostMonitor
    {
        public static float TotalCost = 0;
        public static float LastCost = 0;
        public static GptModelInfo model;

        public static void PrintBaseInfo(GptModelInfo model)
        {
            BasicCostMonitor.model = model;
            int currentTop = Math.Max(Console.CursorTop, 4);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Model:         {model.Name}");
            Console.WriteLine($"TotalCost:     {TotalCost:N6}$");
            Console.WriteLine($"LastPromtCost: {LastCost:N6}$");

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(new string('-', Console.BufferWidth - 1));
            Console.SetCursorPosition(0, currentTop);
        }
    }
}
