using System.Collections.Concurrent;

namespace AspnetcoreEx.KernelService.Tools;

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
                    AppendToJsonl(new object[] { RequestJson, Result, Metadata }, saveFilePath);
                    statusTracker.NumTasksInProgress--;
                    statusTracker.NumTasksFailed++;
                }
            }
            else
            {
                AppendToJsonl(new object[] { RequestJson, responseJson, Metadata }, saveFilePath);
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
                AppendToJsonl(new object[] { RequestJson, Result, Metadata }, saveFilePath);
                statusTracker.NumTasksInProgress--;
                statusTracker.NumTasksFailed++;
            }
        }
    }

    private static void AppendToJsonl(object data, string filename)
    {
        var json = JsonSerializer.Serialize(data);
        File.AppendAllText(filename, json + Environment.NewLine);
    }
}

public class ApiProcessor(HttpClient client, ITokenCounter tokenCounter)
{
    /// <summary>
    ///  await _apiProcessor.ProcessApiRequestsFromFile("requests.jsonl", "result.jsonl", "https://api.xxx.com/v1/endpoint", "your-api-key", 60, 10000, 3);
    /// </summary>
    /// <param name="requestsFilePath"></param>
    /// <param name="saveFilePath"></param>
    /// <param name="requestUrl"></param>
    /// <param name="apiKey"></param>
    /// <param name="maxRequestsPerMinute"></param>
    /// <param name="maxTokensPerMinute"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="tokenEncodingName">cl100k_base</param>
    /// <returns></returns>
    public async Task ProcessApiRequestsFromFile(
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
}