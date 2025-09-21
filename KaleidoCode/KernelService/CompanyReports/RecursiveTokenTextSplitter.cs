
namespace KaleidoCode.KernelService.CompanyReports;

public static class RecursiveTokenTextSplitter
{
    private static readonly List<string> _separators = ["\n\n", "\n", ".", "!", "?", ",", " ", ""];

    public static List<string> TiktokenSplitText(string text, string modelName = "gpt-4o", int chunkSize = 300, int chunkOverlap = 50)
    {
        try
        {
            var encoder = Tiktoken.ModelToEncoder.For(modelName);
            return TokenSplit(
                text,
                t => [.. encoder.Encode(t)],
                encoder.Decode,
                chunkSize,
                chunkOverlap
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Tiktoken split failed", ex);
        }
    }

    public static List<string> SharpTokenSplitText(string text, string modelName = "cl100k_base", int chunkSize = 300, int chunkOverlap = 50)
    {
        try
        {
            var encoder = SharpToken.GptEncoding.GetEncoding(modelName);
            return TokenSplit(
                text,
                t => encoder.Encode(t),
                encoder.Decode,
                chunkSize,
                chunkOverlap
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("SharpToken split failed", ex);
        }
    }

    public static List<string> SmartSplitText(string text, int chunkSize = 300, int chunkOverlap = 50)
    {
        try
        {
            return TiktokenSplitText(text, "gpt-4o", chunkSize, chunkOverlap);
        }
        catch
        {
            try
            {
                return SharpTokenSplitText(text, "cl100k_base", chunkSize, chunkOverlap);
            }
            catch
            {
                return CharacterSplitText(text, chunkSize * 4, chunkOverlap * 4);
            }
        }
    }

    #region private methods
    private static List<string> TokenSplit(
        string text,
        Func<string, List<int>> encode,
        Func<List<int>, string> decode,
        int chunkSize,
        int chunkOverlap)
    {
        if (chunkOverlap >= chunkSize)
            throw new ArgumentException("chunkOverlap must be smaller than chunkSize.");

        var tokens = encode(text);
        var chunks = new List<string>();
        int start = 0;

        while (start < tokens.Count)
        {
            int end = Math.Min(start + chunkSize, tokens.Count);
            var chunkTokens = tokens.Skip(start).Take(end - start).ToList();
            string chunkText = decode(chunkTokens);
            chunks.Add(chunkText);
            start += chunkSize - chunkOverlap;
        }

        return chunks;
    }

    // Character-based splitter and SmartSplitText are unchanged
    public static List<string> CharacterSplitText(string text, int chunkSize = 300, int chunkOverlap = 50)
    {
        var chunks = new List<string>();
        SplitRecursive(text, 0, chunks, chunkSize);
        return AddOverlap(chunks, chunkSize, chunkOverlap);
    }

    private static void SplitRecursive(string text, int sepIdx, List<string> chunks, int chunkSize)
    {
        if (text.Length <= chunkSize || sepIdx >= _separators.Count)
        {
            chunks.Add(text);
            return;
        }

        var sep = _separators[sepIdx];
        var parts = sep == "" ? [] : new List<string>(text.Split(sep));
        if (sep == "")
        {
            // Last level, hard cut by character
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                int len = Math.Min(chunkSize, text.Length - i);
                chunks.Add(text.Substring(i, len));
            }
            return;
        }

        foreach (var part in parts)
        {
            if (part.Length == 0) continue;
            if (part.Length > chunkSize)
                SplitRecursive(part, sepIdx + 1, chunks, chunkSize);
            else
                chunks.Add(part);
        }
    }

    private static List<string> AddOverlap(List<string> chunks, int chunkSize, int chunkOverlap)
    {
        var result = new List<string>();
        int i = 0;
        while (i < chunks.Count)
        {
            string chunk = chunks[i];
            int len = chunk.Length;
            int start = 0;
            while (start < len)
            {
                int end = Math.Min(start + chunkSize, len);
                string subChunk = chunk.Substring(start, end - start);
                result.Add(subChunk);
                if (end == len) break;
                start += chunkSize - chunkOverlap;
            }
            i++;
        }
        return result;
    }

    #endregion
}
