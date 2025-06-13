
namespace AspnetcoreEx.KernelService.CompanyReports;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;

public class AsyncOpenaiProcessor
{
    private readonly OpenAIClient _client;
    private const string DefaultModel = "gpt-4o-mini-2024-07-18";

    public AsyncOpenaiProcessor(OpenAIClient client)
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

    public async Task<Dictionary<int, dynamic>> ProcessStructuredOutputsRequestsAsync(
        string model,
        int temperature,
        string systemContent,
        List<string> queries,
        Type responseFormat,
        bool preserveRequests,
        bool preserveResults,
        int loggingLevel,
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
                seed = (int?)null,
                messages = new[]
                {
                    new { role = "system", content = systemContent },
                    new { role = "user", content = queries[idx] }
                },
                response_format = responseFormat?.Name,
                metadata = new { original_index = idx }
            };
            jsonlRequests.Add(request);
        }

        // 2. Get unique filepaths if files already exist
        requestsFilepath = GetUniqueFilePath(requestsFilepath);
        saveFilepath = GetUniqueFilePath(saveFilepath);

        // 3. Write requests to JSONL file
        await File.WriteAllLinesAsync(requestsFilepath, jsonlRequests.ConvertAll<string>(x => JsonSerializer.Serialize(x)), cancellationToken);

        // 4. Process API requests
        var results = new List<(int index, object question, object answer)>();
        var client = _client.GetChatClient(model ?? DefaultModel).AsIChatClient();

        for (int i = 0; i < jsonlRequests.Count; i++)
        {
            var req = jsonlRequests[i];
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, systemContent),
                new ChatMessage(ChatRole.User, queries[i])
            };
            var options = new ChatOptions
            {
                Temperature = temperature,
                // Seed = null,
                // ResponseFormat = responseFormat?.Name
            };
            try
            {
                var content = await client.GetResponseAsync(messages, options, cancellationToken: cancellationToken);
                string answerContent = content?.Text ?? "";
                object answerObj;
                try
                {
                    answerObj = JsonSerializer.Deserialize(answerContent, responseFormat!) ?? throw new OperationException("data can not be deserialize");
                }
                catch
                {
                    answerObj = answerContent;
                }
                results.Add((i, messages, answerObj));
            }
            catch (Exception ex)
            {
                results.Add((i, messages, $"[ERROR] {ex.Message}"));
            }
        }

        // 5. Write result file
        using (var sw = new StreamWriter(saveFilepath))
        {
            foreach (var r in results)
            {
                var line = JsonSerializer.Serialize(new object[] { r.question, r.answer, new { original_index = r.index } });
                await sw.WriteLineAsync(line);
            }
        }

        // 6. Read and sort the results
        var validatedDataList = new Dictionary<int, dynamic>();
        using (var sr = new StreamReader(saveFilepath))
        {
            string? line;
            while ((line = await sr.ReadLineAsync(cancellationToken)) != null)
            {
                try
                {
                    var arr = JsonSerializer.Deserialize<JsonElement[]>(line);
                    var idx = arr[2].GetProperty("original_index").GetInt32();
                    var answer = arr[1];
                    validatedDataList[idx] = answer;
                }
                catch
                {
                }
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