namespace KaleidoCode.KernelService.CompanyReports;

using System.Text.RegularExpressions;

public partial class ApiHelper
{
    public static string ApiEndpointFromUrl(string requestUrl)
    {
        if (string.IsNullOrEmpty(requestUrl))
            return "";

        // first case：https://xxx/v1/xxx
        var match = MyRegex1().Match(requestUrl);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        // second case：Azure OpenAI URL
        match = MyRegex().Match(requestUrl);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value;
        }

        return "";
    }

    [GeneratedRegex(@"^https://[^/]+/openai/deployments/[^/]+/(.+?)(\?|$)")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"^https://[^/]+/v\d+/(.+)$")]
    private static partial Regex MyRegex1();
}