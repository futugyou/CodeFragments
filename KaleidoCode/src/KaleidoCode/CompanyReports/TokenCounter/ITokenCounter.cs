
namespace CompanyReports.TokenCounter;

public interface ITokenCounter
{
    int Count(string text, string encodingName = "cl100k_base");
}

public static class ITokenCounterExtensions
{
    public static int NumTokensConsumedFromRequest(this ITokenCounter tokenCounter,
        JsonElement requestJson,
        string apiEndpoint,
        string tokenEncodingName)
    {
        // completions
        if (apiEndpoint.EndsWith("completions"))
        {
            int maxTokens = requestJson.TryGetProperty("max_tokens", out var maxTokensElem) && maxTokensElem.TryGetInt32(out var mt) ? mt : 15;
            int n = requestJson.TryGetProperty("n", out var nElem) && nElem.TryGetInt32(out var nn) ? nn : 1;
            int completionTokens = n * maxTokens;

            // chat completions
            if (apiEndpoint.StartsWith("chat/"))
            {
                int numTokens = 0;
                if (requestJson.TryGetProperty("messages", out var messagesElem) && messagesElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var message in messagesElem.EnumerateArray())
                    {
                        numTokens += 4; // every message follows <im_start>{role/name}\n{content}<im_end>\n
                        foreach (var prop in message.EnumerateObject())
                        {
                            numTokens += tokenCounter.Count(prop.Value.GetString() ?? "", tokenEncodingName);
                            if (prop.Name == "name")
                                numTokens -= 1; // role is always required and always 1 token
                        }
                    }
                }
                numTokens += 2; // every reply is primed with <im_start>assistant
                return numTokens + completionTokens;
            }
            // normal completions
            else
            {
                if (requestJson.TryGetProperty("prompt", out var promptElem))
                {
                    if (promptElem.ValueKind == JsonValueKind.String)
                    {
                        int promptTokens = tokenCounter.Count(promptElem.GetString() ?? "", tokenEncodingName);
                        int numTokens = promptTokens + completionTokens;
                        return numTokens;
                    }
                    else if (promptElem.ValueKind == JsonValueKind.Array)
                    {
                        int promptTokens = promptElem.EnumerateArray()
                            .Sum(p => tokenCounter.Count(p.GetString() ?? "", tokenEncodingName));
                        int numTokens = promptTokens + completionTokens * promptElem.GetArrayLength();
                        return numTokens;
                    }
                    else
                    {
                        throw new ArgumentException("Expecting either string or list of strings for \"prompt\" field in completion request");
                    }
                }
                else
                {
                    throw new ArgumentException("Missing \"prompt\" field in completion request");
                }
            }
        }
        // embeddings
        else if (apiEndpoint == "embeddings")
        {
            if (requestJson.TryGetProperty("input", out var inputElem))
            {
                if (inputElem.ValueKind == JsonValueKind.String)
                {
                    int numTokens = tokenCounter.Count(inputElem.GetString() ?? "", tokenEncodingName);
                    return numTokens;
                }
                else if (inputElem.ValueKind == JsonValueKind.Array)
                {
                    int numTokens = inputElem.EnumerateArray()
                        .Sum(i => tokenCounter.Count(i.GetString() ?? "", tokenEncodingName));
                    return numTokens;
                }
                else
                {
                    throw new ArgumentException("Expecting either string or list of strings for \"input\" field in embedding request");
                }
            }
            else
            {
                throw new ArgumentException("Missing \"input\" field in embedding request");
            }
        }
        else
        {
            throw new NotImplementedException($"API endpoint \"{apiEndpoint}\" not implemented in this method");
        }
    }
}
