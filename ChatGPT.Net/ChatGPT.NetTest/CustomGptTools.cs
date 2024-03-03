using System.Diagnostics;
using static ChatGPT.Net.GptApiAttribute;

namespace ChatGPT.NetTest
{
    public class CustomGptTools
    {
        public struct CurrentExpenses
        {
            public float totalCost;
            public float lastMessageCost;
            public string usedModel;
            public float promptCostPer1kToken;
            public float completionCostPer1kToken;
        }

        [GptApiMethod("Get Information about the Expenses of this Chat, and pricing Info about yourself.")]
        public static CurrentExpenses GetCurrentExpenses()
        {
            return new()
            {
                totalCost = BasicCostMonitor.TotalCost,
                lastMessageCost = BasicCostMonitor.LastCost,
                usedModel = BasicCostMonitor.model.Name,
                promptCostPer1kToken = BasicCostMonitor.model.PromptCostPer1kToken,
                completionCostPer1kToken = BasicCostMonitor.model.CompletionCostPer1kToken
            };
        }

        [GptApiMethod("Shutdown a Device like a PC or NOTEBOOK, can't be used to close/control programs. IGNORE the use of synonyms for \"Shutdown\" like \"quit, exit, cancel, etc.\". ONLY REACT TO \"Shutdown\"")]
        [GptApiParam(nameof(deviceID), "The Identification of what to shut down, if the user didn't provide a valid one, then is probably isn't the right tool you should use", false)]
        public static async Task<string> ShutdownPC(string deviceID)
        {
            if (!validDeviceIDs.Contains(deviceID))
                return "Invalid device ID: " + deviceID;

            Console.WriteLine("Shutting down in 8 seconds... Press Enter to abort within 5 seconds.");

            var cancellationTokenSource = new CancellationTokenSource();
            var keyPressTask = Task.Run(() => ListenForKeyPress(cancellationTokenSource.Token));
            var delayTask = Task.Delay(5000);

            var completedTask = await Task.WhenAny(keyPressTask, delayTask);

            if (completedTask == keyPressTask && keyPressTask.Result)
            {
                cancellationTokenSource.Cancel(); // Cancel the delay task if it's still running
                return "Shutdown aborted by user.";
            }
            else
            {
                Process.Start("shutdown", "/s /t 3"); //Uncomment this line to actually perform the shutdown.
                return "Shutting down.";
            }
        }

        private static string[] validDeviceIDs =
        {
            "notebook",
        };

        private static async Task<bool> ListenForKeyPress(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Enter)
                {
                    return true; // User pressed Enter
                }
                await Task.Delay(100, cancellationToken); // Small delay to reduce CPU usage
            }
            return false; // Operation was cancelled
        }
    }
}
