
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchIndexEndpoints
{
    public static void UseOpenSearchIndexEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/index")
                .WithName("Elastic Index");

        agentGroup.MapPost("/older", OrderIndex).WithName("older");
        agentGroup.MapPost("/property_visitor", OrderPropertyVisitorIndex).WithName("property_visitor");
        agentGroup.MapPost("/company", CompanyIndex).WithName("company");
        agentGroup.MapPost("/company_nested", CompanyNestedIndex).WithName("company_nested");
        agentGroup.MapPost("/people", PeopleIndex).WithName("people");
        
        agentGroup.MapPost("/reindex_on_server", ReindexOnServer).WithName("reindex_on_server");
        agentGroup.MapPost("/reindex", Reindex).WithName("reindex");
        agentGroup.MapPost("/create_index_reindex", CreateIndexWithReindex).WithName("create_index_reindex");
        agentGroup.MapPost("/mapping", MappingReindex).WithName("mapping");
    }

    static Task<bool> OrderIndex([FromServices] IndexService esService)
    {
        return esService.OrderIndex();
    }

    static Task<bool> OrderPropertyVisitorIndex([FromServices] IndexService esService)
    {
        return esService.OrderPropertyVisitorIndex();
    }

    static Task<bool> CompanyIndex([FromServices] IndexService esService)
    {
        return esService.CompanyIndex();
    }

    static Task<bool> CompanyNestedIndex([FromServices] IndexService esService)
    {
        return esService.CompanyNestedIndex();
    }

    static Task<bool> PeopleIndex([FromServices] IndexService esService)
    {
        return esService.PeopleIndex();
    }

    static Task<ReindexOnServerResponse> ReindexOnServer([FromServices] ReindexService esService)
    {
        return esService.ReindexOnServer();
    }

    static Task  Reindex([FromServices] ReindexService esService)
    {
        return esService.Reindex();
    }

    static Task  CreateIndexWithReindex([FromServices] ReindexService esService)
    {
        return esService.CreateIndexWithReindex();
    }

    static Task  MappingReindex([FromServices] ReindexService esService)
    {
        return esService.MappingReindex();
    }

}