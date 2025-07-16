
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AspnetcoreEx.KernelService.Internal;

/// <summary>
/// The reason for this `Value cannot be an empty string. (Parameter 'toolCallId')` is that the LLM model does not return the callid in the format of OpenAI,
/// so the id is missing when constructing `ChatHistory` in the [`ProcessNonFunctionToolCalls`](https://github.com/microsoft/semantic-kernel/blob/e44bfb1de01a08448905d1e396f2f54d60ebb179/dotnet/src/Connectors/Connectors.OpenAI/Core/ClientCore.ChatCompletion.cs#L1344),
/// finally, an error is reported in OpenAI.Chat.ToolChatMessage..ctor
/// It cannot be solved by filtering. It seems that the only way to fix it is to patch it through httphandle <see cref="ResponseInterceptorHandler"/>.
/// </summary>
public class ToolCallIdFilter : IAutoFunctionInvocationFilter, IFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        await next(context);
    }

    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        await next(context);
    }
}