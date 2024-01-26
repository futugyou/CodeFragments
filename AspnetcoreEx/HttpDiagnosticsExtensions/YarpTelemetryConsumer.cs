
using Yarp.Telemetry.Consumption;

namespace AspnetcoreEx.HttpDiagnosticsExtensions;
public class YarpTelemetryConsumer: IHttpTelemetryConsumer, INetSecurityTelemetryConsumer
{
    public void OnRequestStart(DateTime timestamp, string scheme, string host, int port, string pathAndQuery, int versionMajor, int versionMinor, HttpVersionPolicy versionPolicy)
    {
        Console.WriteLine($"Request to {host} started at {timestamp}");
    }

    public void OnHandshakeStart(DateTime timestamp, bool isServer, string targetHost)
    {
        Console.WriteLine($"Starting TLS handshake with {targetHost}");
    }
}