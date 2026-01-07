using SemanticKernelStack.Services;
using Microsoft.AspNetCore.Mvc;

namespace SemanticKernelStack.Api;

public static class TokenCounterEndpoints
{
    private const string text = @"The rise of city-states and the division of the empire (185-307),
185: The mayoral system was established in the city of Muse, and border disputes began to emerge,
212: Tulipa (Two Rivers City) adopted a two-chamber parliamentary system and transformed into a regional power.";

    public static void UseTokenCounterEndpoints(this IEndpointRouteBuilder app)
    {
        var agentGroup = app.MapGroup("/api/token")
                .WithName("sk token");

        agentGroup.MapPost("/no-token", NoToken).WithName("no-token");
        agentGroup.MapPost("/sharp", Sharp).WithName("sharp");
        agentGroup.MapPost("/ml-token", MlToken).WithName("ml-token");
        agentGroup.MapPost("/roberta", Roberta).WithName("roberta");
    }

    static List<string> NoToken([FromServices] TokenCounterService service)
    {
        return service.NoToken(text);
    }

    static List<string> Sharp([FromServices] TokenCounterService service)
    {
        return service.Sharp(text);
    }

    static List<string> MlToken([FromServices] TokenCounterService service)
    {
        Assembly assembly = typeof(TokenCounterService).Assembly;
        Stream vocabStream = assembly.GetManifestResourceStream("vocab.bpe")!;
        return service.MlToken(text, vocabStream);
    }

    static List<string> Roberta([FromServices] TokenCounterService service)
    {
        Assembly assembly = typeof(TokenCounterService).Assembly;
        Stream vocabularyStream = assembly.GetManifestResourceStream("encoder.json")!;
        Stream mergeStream = assembly.GetManifestResourceStream("vocab.bpe")!;
        Stream highestOccurrenceMappingStream = assembly.GetManifestResourceStream("dict.txt")!;
        return service.Roberta(text, vocabularyStream, mergeStream, highestOccurrenceMappingStream);
    }
}

