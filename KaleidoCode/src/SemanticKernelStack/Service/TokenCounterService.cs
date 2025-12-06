
using Microsoft.ML.Tokenizers;
using SharpToken;

namespace SemanticKernelStack.Services;

[Experimental("SKEXP0011")]
public class TokenCounterService
{
    public List<string> NoToken(string text)
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40);
        return TextChunker.SplitPlainTextParagraphs(lines, 120); ;
    }

    public List<string> Sharp(string text)
    {
        var lines = TextChunker.SplitPlainTextLines(text, 40, SharpTokenTokenCounter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: SharpTokenTokenCounter);
    }


    public List<string> MlToken(string text, Stream vocabStream)
    {
        var counter = MicrosoftMLTokenCounter(vocabStream);
        var lines = TextChunker.SplitPlainTextLines(text, 40, counter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: counter);
    }

    public List<string> Roberta(string text, Stream vocabularyStream, Stream mergeStream, Stream highestOccurrenceMappingStream)
    {
        var counter = MicrosoftMLRobertaTokenCounter(vocabularyStream, mergeStream, highestOccurrenceMappingStream);
        var lines = TextChunker.SplitPlainTextLines(text, 40, counter);
        return TextChunker.SplitPlainTextParagraphs(lines, 120, tokenCounter: counter);
    }

    // SharpToken token counter
    private static TextChunker.TokenCounter SharpTokenTokenCounter => input =>
    {
        var encoding = GptEncoding.GetEncoding("cl100k_base");
        var tokens = encoding.Encode(input);

        return tokens.Count;
    };

    // Microsoft.ML.Tokenizers token counter
    private static TextChunker.TokenCounter MicrosoftMLTokenCounter(Stream vocabStream) => input =>
    {
        var tokenizer = BpeTokenizer.Create(vocabStream, null);
        return tokenizer.CountTokens(input);
    };

    // Microsoft.ML.Robert token counter
    private static TextChunker.TokenCounter MicrosoftMLRobertaTokenCounter(Stream vocabularyStream, Stream mergeStream, Stream highestOccurrenceMappingStream) => (string input) =>
    {
        var tokenizer = EnglishRobertaTokenizer.Create(
                                   vocabularyStream,
                                   mergeStream,
                                   highestOccurrenceMappingStream,
                                   RobertaPreTokenizer.Instance);
        tokenizer.AddMaskSymbol();
        return tokenizer.CountTokens(input);
    };
}