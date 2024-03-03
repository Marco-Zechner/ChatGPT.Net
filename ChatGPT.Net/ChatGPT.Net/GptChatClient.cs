using Newtonsoft.Json.Linq;

namespace ChatGPT.Net
{
    public class GptChatClient(string apiKey, GptModelInfo model, GptToolConfiguration toolConfig, GptMessage systemInstruction = null, IGptDebug debug = null)
    {
        private readonly string apiKey = apiKey;
        private readonly GptModelInfo model = model;
        public readonly GptToolConfiguration toolConfig = toolConfig;
        private readonly GptMessage systemInstruction = systemInstruction;

        private readonly List<GptMessage> chatHistory = [];
        private readonly GptApiClient apiClient = new(apiKey, model, debug);

        public async Task<GptResponse> AskModelAsync(string message, List<GptTool>? addTools = null, bool useDebug = false)
        {

            var messages = new List<GptMessage>(chatHistory)
            {
                new(GptRole.user, message)
            };
            
            if (systemInstruction != null)
            {
                systemInstruction.Role = "system";
                messages.Add(systemInstruction);
            }

            var availableTools = toolConfig.GetEnabledTools();
            if (addTools != null)
                availableTools.AddRange(addTools);

            GptResponse response = await apiClient.SingleCompletionAsync(messages, availableTools, useDebug);

            if (response is GptErrorResponse)
            {
                if (debug != null && useDebug)
                {
                    debug.Debug((response as GptErrorResponse).Error.Message, 255, 0, 0);
                    return response;
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
                    if (debug != null && useDebug)
                        debug.Debug("Too many Toolcalls.", 255, 0, 255);
                    response = await apiClient.SingleCompletionAsync(messages, null, useDebug);
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
                        if (debug != null && useDebug)
                            debug.Debug("Function {toolCallFormGPT[i].Function.Name} not found.", 255, 0, 255);
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

                        if (debug != null && useDebug)
                            debug.Debug(functionName + "()\n" + JsonHelper.ConvertObjectToString(functionResponse), 0, 255, 255);

                        messages.Add(new GptFunctionReturn(GptRole.tool, JsonHelper.ConvertObjectToString(functionResponse), toolCall[i].ID, toolCall[i].Function.Name));
                    }
                }

                response = await apiClient.SingleCompletionAsync(messages, availableTools, useDebug);
            }

            chatHistory.Add(response.Choices[0].Message);
            return response;
        }
    }
}
