using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class PluginsEndpoints
{
    public static void UsePluginsEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/plugins")
                .WithName("sk plugins");

        agentGroup.MapPost("/google", GoogleSearchAsPlugin).WithName("google");
        agentGroup.MapPost("/dock", Dock).WithName("dock");
        agentGroup.MapPost("/search", WebSearch).WithName("search");
        agentGroup.MapPost("/infr-project-platforms-count", InfrProjectPlatformsCount).WithName("infr-project-platforms-count");
        agentGroup.MapPost("/data-generator", DataGenerator).WithName("data-generator");
        agentGroup.MapPost("/light-controller", LightController).WithName("light-controller");
        agentGroup.MapPost("/email-sender", EmailSender).WithName("email-sender");
        agentGroup.MapPost("/math-executor", MathExecutor).WithName("math-executor");
        agentGroup.MapPost("/word-reader", WordReader).WithName("word-reader");
        agentGroup.MapPost("/file", CallFilePlugin).WithName("file");
        agentGroup.MapPost("/summary", Summary).WithName("summary");
    }

    static async Task<string> GoogleSearchAsPlugin([FromServices] PluginsService service, string input, bool isFunctionCall = false)
    {
        return await service.GoogleSearchAsPlugin(input, isFunctionCall);
    }

    static async Task<IAsyncEnumerable<string>> Dock([FromServices] PluginsService service, string input)
    {
        return await service.Dock(input);
    }
    static async Task<string[]> WebSearch([FromServices] PluginsService service, string input)
    {
        return await service.WebSearch(input);
    }

    static async Task<string[]> InfrProjectPlatformsCount([FromServices] PluginsService service)
    {
        return await service.InfrProjectPlatformsCount();
    }

    static async Task<string[]> DataGenerator([FromServices] PluginsService service, string input)
    {
        return await service.DataGenerator(input);
    }

    static async Task<string[]> LightController([FromServices] PluginsService service)
    {
        return await service.LightController();
    }

    static async Task<string[]> EmailSender([FromServices] PluginsService service)
    {
        return await service.EmailSender();
    }

    static async Task<string[]> MathExecutor([FromServices] PluginsService service)
    {
        return await service.MathExecutor();
    }

    static async Task<string> WordReader([FromServices] PluginsService service, string filePath)
    {
        return await service.WordReader(filePath);
    }

    static async Task<string[]> CallFilePlugin([FromServices] PluginsService service)
    {
        return await service.CallFilePlugin();
    }

    static async Task<string[]> Summary([FromServices] PluginsService service)
    {
        return await service.Summary();
    }
}
