
using Microsoft.AspNetCore.Mvc;

namespace OpenSearchStack.Api;

public static class OpenSearchSearchEndpoints
{
    public static void UseOpenSearchSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/search")
                .WithName("Elastic");

        agentGroup.MapPost("/get_all", GetAll).WithName("get_all");
        agentGroup.MapPost("/get_page", GetPage).WithName("get_page");
        agentGroup.MapPost("/scroll_get", ScrollGet).WithName("scroll_get");
        agentGroup.MapPost("/search", Search).WithName("search");
    }

    static Task GetAll([FromServices] SearchService esService)
    {
        return esService.MatchAll();
    }

    static Task GetPage([FromServices] SearchService esService)
    {
        return esService.PageSearch();
    }

    static Task Search([FromServices] SearchService esService)
    {
        return esService.PageSearch();
    }

    static Task ScrollGet([FromServices] SearchService esService)
    {
        return esService.ScrollingSearch();
    }

}