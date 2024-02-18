using ChatGPT.Net;
using Newtonsoft.Json.Linq;

namespace ChatGPT.NetTest
{
    public class CustomMessage_DEMO
    {
        static GptModelInfo model;

        public static async Task CustomConversation(string KEY, bool enableTools, bool debug = false)
        {
            model = GptModels.GetModel("gpt-3.5-turbo-0125");

            GptApiClient gptClient = new(KEY, model);

            BasicCostMonitor.PrintBaseInfo(model);

            GptToolConfiguration toolConfig = new();

            if (!enableTools)
                toolConfig.DisableAll();

            List<GptMessage> messages = [];

            Console.WriteLine($"CustomConversation_DEMO: {model.Name}");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("User:     ");
                Console.ForegroundColor = ConsoleColor.White;
                messages.Add(new GptMessage(GptRole.user, Console.ReadLine()));

                GptResponse response = await gptClient.SingleCompletionAsync(messages, toolConfig.GetEnabledTools(), debug);

                if (response is GptErrorResponse)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Error:    ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine((response as GptErrorResponse).Error.Message);
                    return;
                }

                int maxToolCalls = model.MaxToolCallsInRow;
                int toolCalls = 0;
                float toolCallCost = 0;
                while (response.Choices[0].FinishReason == "tool_calls")
                {
                    toolCallCost += (response.Usage.PromptTokens * model.PromptCostPer1kToken + response.Usage.CompletionTokens * model.CompletionCostPer1kToken) / 1000;

                    if (toolCalls >= maxToolCalls)
                    {
                        messages.Add(new GptMessage(GptRole.system, $"\"error\": \"To many Toolcalls in a row\"\n\"context\": MaxToolCalls: {maxToolCalls}\n\"reason\": \"ToolCalls: {toolCalls}\""));
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("System: ");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"Too many Toolcalls.");

                        response = await gptClient.SingleCompletionAsync(messages, null, debug);
                        break;
                    }
                    toolCalls++;

                    List<GptToolCall> toolCall = [];

                    List<GptToolCall> toolCallFormGPT = response.Choices[0].Message.ToolCalls;

                    for (int i = 0; i < toolCallFormGPT.Count; i++)
                    {
                        var chatToolCall = new GptToolCall()
                        {
                            Type = "function",
                            Function = toolCallFormGPT[i].Function,
                            ID = toolCallFormGPT[i].ID
                        };

                        if (toolConfig.EnabledToolNames.Contains(toolCallFormGPT[i].Function.Name))
                        {
                            toolCall.Add(chatToolCall);
                        }
                        else
                        {
                            messages.Add(new GptMessage(GptRole.system, $"\"error\": \"Failed Function Call\"\n\"context\": {JsonHelper.ConvertObjectToString(chatToolCall)}\n\"reason\": \"Tool not found.\""));
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("ToolCall: ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine($"Function {toolCallFormGPT[i].Function.Name} not found.");
                        }
                    }

                    if (toolCall.Count > 0)
                    {
                        messages.Add(new GptFunctionCalls(toolCall));

                        for (int i = 0; i < toolCall.Count; i++)
                        {
                            var functionName = toolCall[i].Function.Name;
                            var functionArgs = JObject.Parse(toolCall[i].Function.Arguments);
                            var functionResponse = GptToolHandler.CallFunction(functionName, functionArgs);

                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write("ToolCall: ");
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine(functionName + "()");
                            Console.WriteLine(JsonHelper.ConvertObjectToString(functionResponse));

                            messages.Add(new GptFunctionReturn(GptRole.tool, JsonHelper.ConvertObjectToString(functionResponse), toolCall[i].ID, toolCall[i].Function.Name));
                        }
                    }

                    response = await gptClient.SingleCompletionAsync(messages, toolConfig.GetEnabledTools(), debug);
                }

                BasicCostMonitor.LastCost = toolCallCost;
                BasicCostMonitor.TotalCost += BasicCostMonitor.LastCost;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("GPT:      ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(response.Choices[0].Message.Content);
                
                messages.Add(response.Choices[0].Message);
                BasicCostMonitor.LastCost = (response.Usage.PromptTokens * model.PromptCostPer1kToken + response.Usage.CompletionTokens * model.CompletionCostPer1kToken) / 1000;
                BasicCostMonitor.TotalCost += BasicCostMonitor.LastCost;

                BasicCostMonitor.PrintBaseInfo(model);
                Console.WriteLine();
            }
        }
    }
}
