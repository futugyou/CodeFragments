
using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class PromptEndpoints
{
    public static void UsePromptEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/prompt")
                .WithName("sk prompt");

        agentGroup.MapPost("/base", PromptBase).WithName("base");
        agentGroup.MapPost("/liquid", Liquid).WithName("liquid");
        agentGroup.MapPost("/semantic-kernel", SemanticKernelTemplate).WithName("semantic-kernel");
        agentGroup.MapPost("/handlebars", PromptHandlebars).WithName("handlebars");
        agentGroup.MapPost("/yaml", PromptYAML).WithName("yaml");
    }

    static async Task<string[]> PromptBase([FromServices] PromptService service)
    {
        return await service.PromptBase();
    }

    static async Task<string[]> Liquid([FromServices] PromptService service)
    {
        return await service.Liquid();
    }

    static async Task<string[]> SemanticKernelTemplate([FromServices] PromptService service)
    {
        return await service.SemanticKernelTemplate();
    }

    static async Task<string[]> PromptHandlebars([FromServices] PromptService service)
    {
        return await service.PromptHandlebars();
    }

    static async Task<string[]> PromptYAML([FromServices] PromptService service)
    {
        return await service.PromptYAML();
    }
}
