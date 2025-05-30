namespace AspnetcoreEx.KernelService.Tools;

public interface ITokenCounter
{
    int Count(string text);
}

public class SharpTokenCounter : ITokenCounter
{
    public int Count(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var encoding = SharpToken.GptEncoding.GetEncoding("cl100k_base");
        var tokens = encoding.Encode(text);

        return tokens.Count;
    }
}