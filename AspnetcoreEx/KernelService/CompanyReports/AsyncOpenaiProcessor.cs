
namespace AspnetcoreEx.KernelService.CompanyReports;

using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using OpenAI;
using Path = System.IO.Path;

public class AsyncOpenaiProcessor
{
    private readonly OpenAIClient _client;
    private const string DefaultModel = "gpt-4o-mini-2024-07-18";

    public AsyncOpenaiProcessor([FromKeyedServices("report")] OpenAIClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "OpenAIClient cannot be null.");
    }

    private static string GetUniqueFilePath(string baseFilePath)
    {
        if (!File.Exists(baseFilePath))
            return baseFilePath;

        var baseName = Path.GetFileNameWithoutExtension(baseFilePath);
        var ext = Path.GetExtension(baseFilePath);
        var dir = Path.GetDirectoryName(baseFilePath) ?? "";
        int counter = 1;
        string newPath;
        do
        {
            newPath = Path.Combine(dir, $"{baseName}_{counter}{ext}");
            counter++;
        } while (File.Exists(newPath));
        return newPath;
    }

    public async Task<List<StructuredResult<T>>> ProcessStructuredOutputsRequestsAsync<T>(
        string model,
        int temperature,
        string systemContent,
        List<string> queries,
        bool preserveRequests,
        bool preserveResults,
        string requestsFilepath,
        string saveFilepath,
        CancellationToken cancellationToken)
    {
        // 1. Create requests for jsonl
        var jsonlRequests = new List<object>();
        for (int idx = 0; idx < queries.Count; idx++)
        {
            var request = new
            {
                model,
                temperature,
                messages = new[]
                {
                    new { role = "system", content = Prompts.BuildSystemPrompt<T> (systemContent) },
                    new { role = "user", content = queries[idx] }
                },
                metadata = new { original_index = idx }
            };
            jsonlRequests.Add(request);
        }

        // 2. Get unique filepaths if files already exist
        requestsFilepath = GetUniqueFilePath(requestsFilepath);
        saveFilepath = GetUniqueFilePath(saveFilepath);

        // 3. Write requests to JSONL file
        await File.WriteAllLinesAsync(requestsFilepath, jsonlRequests.ConvertAll(x => JsonSerializer.Serialize(x)), cancellationToken);

        // 4. Process API requests
        var results = new ConcurrentBag<StructuredResult<T>>();
        var client = _client.GetChatClient(model ?? DefaultModel).AsIChatClient();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        await Parallel.ForEachAsync(
            Enumerable.Range(0, jsonlRequests.Count),
            parallelOptions,
            async (i, ct) =>
            {
                var req = jsonlRequests[i];
                var messages = new[]
                {
                    new ChatMessage(ChatRole.System, Prompts.BuildSystemPrompt<T> (systemContent)),
                    new ChatMessage(ChatRole.User, queries[i])
                };
                var options = new ChatOptions
                {
                    Temperature = temperature,
                    ResponseFormat = ChatResponseFormat.Json,
                };
                try
                {
                    var content = await client.GetResponseAsync(messages, options, cancellationToken: ct);
                    string answerContent = content?.Text ?? "";
                    T? answerObj = default;
                    try
                    {
                        answerObj = JsonSerializer.Deserialize<T>(answerContent);
                    }
                    catch
                    {
                    }
                    results.Add(new() { Index = i, ChatMessages = messages, Answer = answerObj, AnswerRaw = answerContent });
                }
                catch (Exception ex)
                {
                    results.Add(new() { Index = i, ChatMessages = messages, Error = $"[ERROR] {ex.Message}" });
                }
            }
        );

        // 5. Write result file
        string line;
        using var sw = new StreamWriter(saveFilepath);
        foreach (var item in results)
        {
            line = JsonSerializer.Serialize(item);
            await sw.WriteLineAsync(line);
        }

        // 6. Read and sort the results
        List<StructuredResult<T>> validatedDataList = [];
        using var sr = new StreamReader(saveFilepath);
        while ((line = await sr.ReadLineAsync(cancellationToken) ?? "") != "")
        {
            try
            {
                var arr = JsonSerializer.Deserialize<StructuredResult<T>>(line);
                if (arr != null)
                {
                    validatedDataList.Add(arr);
                }
            }
            catch
            {
            }
        }

        // 7. Cleaning up temporary files
        if (!preserveRequests && File.Exists(requestsFilepath))
            File.Delete(requestsFilepath);
        if (!preserveResults && File.Exists(saveFilepath))
            File.Delete(saveFilepath);

        return validatedDataList;
    }
}

public class StructuredResult<T>
{
    [JsonPropertyName("original_index")]
    public int Index { get; set; }
    [JsonPropertyName("question")]
    public ChatMessage[] ChatMessages { get; set; }
    [JsonPropertyName("answer_content")]
    public string AnswerRaw { get; set; } = "";
    [JsonPropertyName("answer")]
    public T? Answer { get; set; }
    [JsonPropertyName("error")]
    public string Error { get; set; } = "";
}