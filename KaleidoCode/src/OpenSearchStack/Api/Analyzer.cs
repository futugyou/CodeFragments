
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchAnalyzerEndpoints
{
    public static void UseOpenSearchAnalyzerEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/analyzer")
                .WithName("Elastic");

        agentGroup.MapPost("/cteaer", Analyzer).WithName("creater");
        agentGroup.MapPost("/testing", AnalyzerTesting).WithName("testing");
    }

    static Task<CreateIndexResponse> Analyzer([FromServices] AnalyzerService esService, string type = "base")
    {
        return type == "base" ? esService.CreateBaseAnalyzer() : esService.CreateCustomAnalyzer();
    }

    static IAsyncEnumerable<string> AnalyzerTesting([FromServices] TestAnalyzerService esService)
    {
        return esService.TestAnalyzer();
    }
}