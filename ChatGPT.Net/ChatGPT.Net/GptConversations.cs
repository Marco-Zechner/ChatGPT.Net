using System.Collections.Concurrent;

namespace ChatGPT.Net
{
    public class GptConversations
    {
        private static readonly ConcurrentDictionary<string, GptConversation> conversations = new();

        private static GptConversation GetOrCreateConversation(string conversationID)
        {
            return conversations.GetOrAdd(conversationID, _ => new GptConversation());
        }

        public static void AddMessage(string conversationID, GptMessage message)
        {
            var conversation = GetOrCreateConversation(conversationID);
            conversation.Messages.Add(message);
        }        
        
        public static void AddResponse(string conversationID, GptResponse response)
        {
            var conversation = GetOrCreateConversation(conversationID);
            conversation.Messages.Add(response.Choices[0].Message);

            GptModelInfo model = GptModels.GetModel(response.Model);

            conversation.LastCost = (response.Usage.PromptTokens * model.PromptCostPer1kToken + response.Usage.CompletionTokens * model.CompletionCostPer1kToken) / 1000;
        }

        public static IReadOnlyList<GptMessage> GetMessages(string conversationID)
        {
            return GetOrCreateConversation(conversationID).Messages.AsReadOnly();
        }

        public static double GetTotalCost(string conversationID)
        {
            return GetOrCreateConversation(conversationID).TotalCost;
        }

        public static double GetLastCost(string conversationID)
        {
            return GetOrCreateConversation(conversationID).LastCost;
        }
    }

    public class GptConversation
    {
        public List<GptMessage> Messages { get; } = [];
        public double TotalCost { get; private set; }
        private double lastCost;

        public double LastCost
        {
            get => lastCost;
            set
            {
                lastCost = value;
                TotalCost += lastCost;
            }
        }
    }
}
