using System.Text.Json.Nodes;

namespace AspnetcoreEx.KernelService;

public class ResponseInterceptorHandler : DelegatingHandler
{
    public ResponseInterceptorHandler() : base(new HttpClientHandler())
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Send the original request
        var response = await base.SendAsync(request, cancellationToken);

        // Read the original response content
        var originalContent = await response.Content.ReadAsStringAsync();

        var originalJson = JsonSerializer.Deserialize<JsonNode>(originalContent);

        //Set the id to a random value if not assigned a value
        var choices = originalJson?["choices"] as JsonArray ?? new JsonArray();
        foreach (var choice in choices)
        {
            var message = choice?["message"] as JsonObject;
            var toolCalls = message?["tool_calls"] as JsonArray ?? new JsonArray();
            foreach (var toolCall in toolCalls)
            {
                var toolCallObj = toolCall as JsonObject;
                if (toolCallObj != null && toolCallObj.ContainsKey("id") && string.IsNullOrEmpty(toolCallObj["id"]?.GetValue<string>()))
                {
                    string newId = Guid.NewGuid().ToString();
                    toolCallObj["id"] = newId;
                }
            }
        }

        var modifiedContent = originalJson?.ToJsonString() ?? "";

        // Create a new HttpResponseMessage with modified content
        var modifiedResponse = new HttpResponseMessage(response.StatusCode)
        {
            Content = new StringContent(modifiedContent, Encoding.UTF8, response.Content?.Headers?.ContentType?.MediaType)
        };

        // Copy headers and other properties from original response
        foreach (var header in response.Headers)
        {
            modifiedResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (response.Content != null)
        {
            foreach (var contentHeader in response.Content.Headers)
            {
                modifiedResponse.Content?.Headers.TryAddWithoutValidation(contentHeader.Key, contentHeader.Value);
            }
        }

        return modifiedResponse;
    }
}