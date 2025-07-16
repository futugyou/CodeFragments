
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AspnetcoreEx.KernelService.Internal;

public class ToolCallIdFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        if (string.IsNullOrEmpty(context.ToolCallId))
        {
            var metadata = context.ChatMessageContent.Metadata;
            string? toolCallId = null;

            if (metadata?.TryGetValue(OpenAIChatMessageContent.ToolIdProperty, out var toolIdObj) == true)
            {
                toolCallId = toolIdObj?.ToString();
            }

            if (string.IsNullOrEmpty(toolCallId) &&
                metadata?.TryGetValue("Id", out var idObj) == true &&
                idObj?.ToString() is string idStr)
            {
                toolCallId = idStr;
            }

            toolCallId ??= Guid.NewGuid().ToString();

            var newMetadata = metadata?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? [];
            newMetadata[OpenAIChatMessageContent.ToolIdProperty] = toolCallId;
            context.ChatMessageContent.Metadata = newMetadata;
            for (int i = 0; i < context.ChatMessageContent.Items.Count; i++)
            {
                if (context.ChatMessageContent.Items[i] is not FunctionResultContent resultContent)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(resultContent.CallId))
                {
                    context.ChatMessageContent.Items[i] = new FunctionResultContent(
                         functionName: resultContent.FunctionName,
                         pluginName: resultContent.PluginName,
                         callId: toolCallId,
                         result: resultContent.Result
                     );
                }
            }

            context = new(context.Kernel, context.Function, context.Result, context.ChatHistory, context.ChatMessageContent)
            {
                ToolCallId = toolCallId,
                Arguments = context.Arguments,
                RequestSequenceIndex = context.RequestSequenceIndex,
                FunctionSequenceIndex = context.FunctionSequenceIndex,
                ExecutionSettings = context.ExecutionSettings,
            };
        }


        await next(context);
    }
}