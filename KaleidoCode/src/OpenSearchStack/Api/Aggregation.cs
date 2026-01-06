
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchAggregationEndpoints
{
    public static void UseOpenSearchAggregationEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/aggregation")
                .WithName("Elastic Aggregation");

        agentGroup.MapPost("/terms", Terms).WithName("terms");
        agentGroup.MapPost("/averageMax", AverageMax).WithName("averageMax");
    }

    static Task<AggregateDictionary> Terms([FromServices] AggregationSerice esService)
    {
        return esService.Terms();
    }

    static Task<AggregateDictionary> AverageMax([FromServices] AggregationSerice esService)
    {
        return esService.AverageMax();
    }

}