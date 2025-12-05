using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace KaleidoCode.KernelService.CompanyReports;

public class StatusTracker
{
    public int NumTasksStarted = 0;
    public int NumTasksInProgress = 0;
    public int NumTasksSucceeded = 0;
    public int NumTasksFailed = 0;
    public int NumRateLimitErrors = 0;
    public int NumApiErrors = 0;
    public int NumOtherErrors = 0;
    public DateTime TimeOfLastRateLimitError = DateTime.MinValue;
}

internal class APIRequestMarshal
{
    [JsonPropertyName("request_json")]
    public JsonElement? RequestJson { get; set; }
    [JsonPropertyName("response_json")]
    public JsonElement? ResponseJson { get; set; }
    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; set; }
    [JsonPropertyName("result")]
    public List<object> Result { get; set; } = [];

    public APIRequestMarshal() { }

    [JsonConstructor]
    public APIRequestMarshal(JsonElement? requestJson, JsonElement? responseJson, JsonElement? metadata, List<object>? result)
    {
        RequestJson = requestJson;
        ResponseJson = responseJson;
        Metadata = metadata;
        Result = result ?? [];
    }
}

public class APIRequest
{
    public int TaskId { get; set; }
    public JsonElement RequestJson { get; set; }
    public int TokenConsumption { get; set; }
    public int AttemptsLeft { get; set; }
    public JsonElement? Metadata { get; set; }
    public List<object> Result { get; set; } = [];

    public async Task CallApiAsync(
        HttpClient client,
        string requestUrl,
        Dictionary<string, string> requestHeader,
        ConcurrentQueue<APIRequest> retryQueue,
        string saveFilePath,
        StatusTracker statusTracker)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(RequestJson.GetRawText(), Encoding.UTF8, "application/json")
            };
            foreach (var kv in requestHeader)
                request.Headers.Add(kv.Key, kv.Value);

            var response = await client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseString).RootElement;

            if (responseJson.TryGetProperty("error", out var error))
            {
                statusTracker.NumApiErrors++;
                if (error.GetProperty("message").GetString()?.ToLower().Contains("rate limit") ?? false)
                {
                    statusTracker.TimeOfLastRateLimitError = DateTime.Now;
                    statusTracker.NumRateLimitErrors++;
                    statusTracker.NumApiErrors--;
                }
                Result.Add(error);
                if (AttemptsLeft > 0)
                    retryQueue.Enqueue(this);
                else
                {
                    AppendToJsonl(new APIRequestMarshal(RequestJson, responseJson, Metadata, null), saveFilePath);
                    statusTracker.NumTasksInProgress--;
                    statusTracker.NumTasksFailed++;
                }
            }
            else
            {
                AppendToJsonl(new APIRequestMarshal(RequestJson, responseJson, Metadata, null), saveFilePath);
                statusTracker.NumTasksInProgress--;
                statusTracker.NumTasksSucceeded++;
            }
        }
        catch (Exception ex)
        {
            statusTracker.NumOtherErrors++;
            Result.Add(ex.ToString());
            if (AttemptsLeft > 0)
                retryQueue.Enqueue(this);
            else
            {
                AppendToJsonl(new APIRequestMarshal(RequestJson, null, Metadata, Result), saveFilePath);
                statusTracker.NumTasksInProgress--;
                statusTracker.NumTasksFailed++;
            }
        }
    }

    private static void AppendToJsonl(APIRequestMarshal data, string filename)
    {
        var json = JsonSerializer.Serialize(data);
        File.AppendAllText(filename, json + Environment.NewLine);
    }
}

public class ParallelProcessorManager
{
    /// <summary>
    ///  await manager.ProcessApiRequestsFromFile(httpClient, tokenCounter, "requests.jsonl", "result.jsonl", "https://api.xxx.com/v1/endpoint", "your-api-key", 60, 10000, 3);
    /// </summary>
    /// <returns></returns>
    public static async Task ProcessApiRequestsFromFile(
        HttpClient client,
        ITokenCounter tokenCounter,
        string requestsFilePath,
        string saveFilePath,
        string requestUrl,
        string apiKey,
        double maxRequestsPerMinute,
        double maxTokensPerMinute,
        int maxAttempts,
        string tokenEncodingName = "cl100k_base")
    {
        var requestHeader = new Dictionary<string, string> { { "Authorization", $"Bearer {apiKey}" } };
        if (requestUrl.Contains("/deployments"))
            requestHeader = new Dictionary<string, string> { { "api-key", apiKey } };

        var queueOfRequestsToRetry = new ConcurrentQueue<APIRequest>();
        var statusTracker = new StatusTracker();
        var availableRequestCapacity = maxRequestsPerMinute;
        var availableTokenCapacity = maxTokensPerMinute;
        var lastUpdateTime = DateTime.Now;
        var fileNotFinished = true;
        APIRequest? nextRequest = null;
        var requests = File.ReadLines(requestsFilePath).GetEnumerator();
        var endpoint = ApiHelper.ApiEndpointFromUrl(requestUrl);

        var tasks = new List<Task>();

        while (true)
        {
            if (nextRequest == null)
            {
                if (queueOfRequestsToRetry.TryDequeue(out var retryReq))
                {
                    nextRequest = retryReq;
                }
                else if (fileNotFinished)
                {
                    if (requests.MoveNext())
                    {
                        var requestJson = JsonDocument.Parse(requests.Current).RootElement;
                        nextRequest = new APIRequest
                        {
                            TaskId = statusTracker.NumTasksStarted,
                            RequestJson = requestJson,
                            TokenConsumption = tokenCounter.NumTokensConsumedFromRequest(requestJson, endpoint, tokenEncodingName),
                            AttemptsLeft = maxAttempts,
                            Metadata = requestJson.TryGetProperty("metadata", out var meta) ? meta : null
                        };
                        statusTracker.NumTasksStarted++;
                        statusTracker.NumTasksInProgress++;
                    }
                    else
                    {
                        fileNotFinished = false;
                    }
                }
            }

            var now = DateTime.Now;
            var secondsSinceUpdate = (now - lastUpdateTime).TotalSeconds;
            availableRequestCapacity = Math.Min(availableRequestCapacity + maxRequestsPerMinute * secondsSinceUpdate / 60.0, maxRequestsPerMinute);
            availableTokenCapacity = Math.Min(availableTokenCapacity + maxTokensPerMinute * secondsSinceUpdate / 60.0, maxTokensPerMinute);
            lastUpdateTime = now;

            if (nextRequest != null)
            {
                if (availableRequestCapacity >= 1 && availableTokenCapacity >= nextRequest.TokenConsumption)
                {
                    availableRequestCapacity -= 1;
                    availableTokenCapacity -= nextRequest.TokenConsumption;
                    nextRequest.AttemptsLeft -= 1;
                    tasks.Add(nextRequest.CallApiAsync(client, requestUrl, requestHeader, queueOfRequestsToRetry, saveFilePath, statusTracker));
                    nextRequest = null;
                }
            }

            if (statusTracker.NumTasksInProgress == 0 && !fileNotFinished && queueOfRequestsToRetry.IsEmpty)
                break;

            await Task.Delay(1);
        }

        await Task.WhenAll(tasks);
    }

    public static int NumTokensConsumedFromRequest(
        ITokenCounter tokenCounter,
        Dictionary<string, object> requestJson,
        string apiEndpoint,
        string tokenEncodingName)
    {
        // completions
        if (apiEndpoint.EndsWith("completions"))
        {
            int maxTokens = requestJson.ContainsKey("max_tokens") ? Convert.ToInt32(requestJson["max_tokens"]) : 15;
            int n = requestJson.ContainsKey("n") ? Convert.ToInt32(requestJson["n"]) : 1;
            int completionTokens = n * maxTokens;

            // chat completions
            if (apiEndpoint.StartsWith("chat/"))
            {
                int numTokens = 0;
                var messages = (List<Dictionary<string, object>>)requestJson["messages"];
                foreach (var message in messages)
                {
                    numTokens += 4;
                    foreach (var kv in message)
                    {
                        numTokens += tokenCounter.Count(kv.Value.ToString()!, tokenEncodingName);
                        if (kv.Key == "name")
                            numTokens -= 1;
                    }
                }
                numTokens += 2; // assistant priming
                return numTokens + completionTokens;
            }
            // normal completions
            else
            {
                var prompt = requestJson["prompt"];
                if (prompt is string promptStr)
                {
                    int promptTokens = tokenCounter.Count(promptStr, tokenEncodingName);
                    return promptTokens + completionTokens;
                }
                else if (prompt is List<object> promptList)
                {
                    int promptTokens = promptList.Sum(p => tokenCounter.Count(p.ToString()!, tokenEncodingName));
                    return promptTokens + completionTokens * promptList.Count;
                }
                else
                {
                    throw new ArgumentException("Expecting either string or list of strings for \"prompt\" field in completion request");
                }
            }
        }
        // embeddings
        else if (apiEndpoint == "embeddings")
        {
            var input = requestJson["input"];
            if (input is string inputStr)
            {
                return tokenCounter.Count(inputStr, tokenEncodingName);
            }
            else if (input is List<object> inputList)
            {
                return inputList.Sum(i => tokenCounter.Count(i.ToString()!, tokenEncodingName));
            }
            else
            {
                throw new ArgumentException("Expecting either string or list of strings for \"inputs\" field in embedding request");
            }
        }
        else
        {
            throw new NotImplementedException($"API endpoint \"{apiEndpoint}\" not implemented in this script");
        }
    }
}