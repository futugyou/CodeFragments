

using KernelMemoryStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace KernelMemoryStack.Api;

public static class ImportEndpoints
{
    public static void UseImportEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/memory")
                .WithName("km memory");

        agentGroup.MapPost("/memclientweb", ClientImportWeb).WithName("memclientweb");
        agentGroup.MapPost("/memweb", ImportWeb).WithName("memweb");
    }

    static async Task<string> ClientImportWeb([FromServices] WebImportService service, string url, string documentId, string question)
    {
        return await service.ClientImportWeb(url, documentId, question);
    }

    static async Task<string> ImportWeb([FromServices] WebImportService service, string url, string documentId, string question)
    {
        return await service.ImportWeb(url, documentId, question);
    }

}
