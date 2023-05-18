using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace AspnetcoreEx.SemanticKernel.Skills;

public class MarkupSkill
{
    [SKFunction("Run Markup")]
    [SKFunctionName("RunMarkup")]
    public async Task<SKContext> RunMarkupAsync(SKContext context)
    {
        var docString = context.Variables.Input;
        var plan = docString.FromMarkup("Run a piece of xml markup", context);

        var result = await plan.InvokeAsync();
        context.Variables.Update(result.Result);
        return context;
    }
}
