
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchInsertEndpoints
{
    public static void UseOpenSearchInsertEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/insert")
                .WithName("Elastic");

        agentGroup.MapPost("/insert", InsertData).WithName("insert");
        agentGroup.MapPost("/update", UpdateData).WithName("update");
        agentGroup.MapPost("/multiple", InsertManyData).WithName("multiple");
        agentGroup.MapPost("/bulk", Bulk).WithName("bulk");
        agentGroup.MapPost("/simple_bulk", SimpleBulk).WithName("simple_bulk");
        agentGroup.MapPost("/complex_bulk", ComplexBulk).WithName("complex_bulk");
    }

    static Task<IndexResponse> InsertData([FromServices] InsertService esService)
    {
        return esService.InsertData();
    }

    static Task<OrderInfo> UpdateData([FromServices] InsertService esService)
    {
        return esService.UpdateData();
    }

    static IAsyncEnumerable<string> InsertManyData([FromServices] InsertService esService)
    {
        return esService.InsertManyData();
    }

    static IAsyncEnumerable<string> Bulk([FromServices] InsertService esService)
    {
        return esService.Bulk();
    }

    static IAsyncEnumerable<string> SimpleBulk([FromServices] InsertService esService)
    {
        return esService.SimpleBulk();
    }

    static IAsyncEnumerable<string> ComplexBulk([FromServices] InsertService esService)
    {
        return esService.ComplexBulk();
    }
}