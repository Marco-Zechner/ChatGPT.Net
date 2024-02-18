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
    }
}
