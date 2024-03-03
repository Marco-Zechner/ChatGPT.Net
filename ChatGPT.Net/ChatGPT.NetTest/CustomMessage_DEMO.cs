using ChatGPT.Net;
using Newtonsoft.Json.Linq;

namespace ChatGPT.NetTest
{
    public class CustomMessage_DEMO
    {
        static GptModelInfo model;

        public static async Task CustomConversation(string KEY, bool enableTools, IGptDebug debug = null)
        {
            model = GptModels.GetModel("gpt-3.5-turbo-0125");

            GptApiClient gptClient = new(KEY, model, debug);

            BasicCostMonitor.PrintBaseInfo(model);

            GptToolConfiguration toolConfig = new();

            if (!enableTools)
                toolConfig.DisableAll();

            List<GptMessage> messages = [];

            Console.WriteLine($"CustomConversation_DEMO: {model.Name}");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("User: ");
                Console.ForegroundColor = ConsoleColor.White;
                messages.Add(new GptMessage(GptRole.user, Console.ReadLine()));

                GptResponse response = await gptClient.SingleCompletionAsync(messages, toolConfig.GetEnabledTools(), debug != null);

                if (response is GptErrorResponse)
                {
                    if (debug != null)
                    {
                        debug.Debug((response as GptErrorResponse).Error.Message, 255, 0, 0);
                        return;
                    }

                    throw new Exception((response as GptErrorResponse).Error.Message);
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

                        debug?.Debug("System: Too many Toolcalls.", 255, 0, 255);

                        response = await gptClient.SingleCompletionAsync(messages, null, debug != null);
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


                            debug?.Debug($"System: Function {toolCallFormGPT[i].Function.Name} not found.", 255, 0, 255);
                        }
                    }

                    if (toolCall.Count > 0)
                    {
                        messages.Add(new GptFunctionCalls(toolCall));

                        for (int i = 0; i < toolCall.Count; i++)
                        {
                            var functionName = toolCall[i].Function.Name;
                            var functionArgs = JObject.Parse(toolCall[i].Function.Arguments);
                            var functionResponse = await GptToolHandler.CallFunctionAuto(functionName, functionArgs);

                            debug?.Debug($"ToolCall: {functionName}()\n{JsonHelper.ConvertObjectToString(functionResponse)}", 0, 255, 255);

                            messages.Add(new GptFunctionReturn(GptRole.tool, JsonHelper.ConvertObjectToString(functionResponse), toolCall[i].ID, toolCall[i].Function.Name));
                        }
                    }

                    response = await gptClient.SingleCompletionAsync(messages, toolConfig.GetEnabledTools(), debug != null);
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
