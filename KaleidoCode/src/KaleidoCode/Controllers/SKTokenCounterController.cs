
using System.Reflection;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Text;
using SharpToken;

namespace KaleidoCode.Controllers;

[Experimental("SKEXP0011")]
[Route("api/sk/token")]
[ApiController]
public class SKTokenCounterController : ControllerBase
{
    private const string text = @"The rise of city-states and the division of the empire (185-307),
185: The mayoral system was established in the city of Muse, and border disputes began to emerge,
212: Tulipa (Two Rivers City) adopted a two-chamber parliamentary system and transformed into a regional power.";

    public SKTokenCounterController()
    {
    }

    [Route("no-token")]
    [HttpGet]
    public List<string> NoToken()
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40);
        return TextChunker.SplitPlainTextParagraphs(lines, 120); ;
    }

    [Route("sharp")]
    [HttpGet]
    public List<string> Sharp()
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40, SharpTokenTokenCounter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: SharpTokenTokenCounter);
    }

    [Route("ml-token")]
    [HttpGet]
    public List<string> MlToken()
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40, MicrosoftMLTokenCounter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: MicrosoftMLTokenCounter);
    }

    [Route("ml-roberta")]
    [HttpGet]
    public List<string> Roberta()
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40, MicrosoftMLRobertaTokenCounter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: MicrosoftMLRobertaTokenCounter);
    }

    // SharpToken token counter
    private static TextChunker.TokenCounter SharpTokenTokenCounter => input =>
    {
        var encoding = GptEncoding.GetEncoding("cl100k_base");
        var tokens = encoding.Encode(input);

        return tokens.Count;
    };

    // Microsoft.ML.Tokenizers token counter
    private static TextChunker.TokenCounter MicrosoftMLTokenCounter => input =>
    {
        Assembly assembly = typeof(SKTokenCounterController).Assembly;
        var tokenizer = BpeTokenizer.Create(assembly.GetManifestResourceStream("vocab.bpe")!, null);
        return tokenizer.CountTokens(input);
    };

    // Microsoft.ML.Robert token counter
    private static TextChunker.TokenCounter MicrosoftMLRobertaTokenCounter => input =>
    {
        Assembly assembly = typeof(SKTokenCounterController).Assembly;

        var tokenizer = EnglishRobertaTokenizer.Create(
                                   assembly.GetManifestResourceStream("encoder.json")!,
                                   assembly.GetManifestResourceStream("vocab.bpe")!,
                                   assembly.GetManifestResourceStream("dict.txt")!,
                                   RobertaPreTokenizer.Instance);
        tokenizer.AddMaskSymbol();
        return tokenizer.CountTokens(input);
    };
}
