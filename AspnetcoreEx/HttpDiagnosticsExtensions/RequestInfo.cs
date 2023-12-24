namespace AspnetcoreEx.HttpDiagnosticsExtensions;

public sealed class RequestInfo
{
    private static readonly AsyncLocal<RequestInfo> _asyncLocal = new();
    public static RequestInfo Current => _asyncLocal.Value ??= new();

    public string? RemoteAddress;
    public long ConnectionId;
}
