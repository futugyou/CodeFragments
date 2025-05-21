using System.Buffers;
using System.Text;
using Dapr.Proto.Internals.V1;
using Google.Protobuf.Collections;
using Grpc.Core;

namespace Messaging;

public static class Util
{
    // Maximum size, in bytes, for the buffer used by CallLocalStream: 2KB.
    public const int StreamBufferSize = 2 << 10;

    // MIME media types
    public const string GRPCContentType = "application/grpc";
    public const string JSONContentType = "application/json";
    public const string ProtobufContentType = "application/x-protobuf";
    public const string OctetStreamContentType = "application/octet-stream";

    // HTTP header keys
    public const string ContentTypeHeader = "content-type";
    public const string ContentLengthHeader = "content-length";

    // Metadata prefix and suffix
    public const string DaprHeaderPrefix = "dapr-";
    public const string GRPCBinaryMetadataSuffix = "-bin";

    // Header for invoked app ID
    public const string DestinationIDHeader = "destination-app-id";

    // ErrorInfo metadata value is limited to 64 chars
    public const int MaxMetadataValueLen = 63;

    // ErrorInfo metadata for HTTP response
    public const string ErrorInfoDomain = "dapr.io";
    public const string ErrorInfoHTTPCodeMetadata = "http.code";
    public const string ErrorInfoHTTPErrorMetadata = "http.error_message";

    // Caller/Callee headers
    public const string CallerIDHeader = DaprHeaderPrefix + "caller-app-id";
    public const string CalleeIDHeader = DaprHeaderPrefix + "callee-app-id";

    public static Grpc.Core.Metadata InternalMetadataToGrpcMetadata(MapField<string, ListStringValue> internalMD, bool httpHeaderConversion, CancellationToken cancel)
    {
        string traceparentValue, tracestateValue, grpctracebinValue;

        var md = new Grpc.Core.Metadata();
        foreach (var item in internalMD)
        {
            var k = item.Key;
            var listVal = item.Value;
            var keyName = k.ToLower();
            // get both the trace headers for HTTP/GRPC and continue
            switch (keyName)
            {
                case "traceparent":
                    traceparentValue = listVal?.Values?.FirstOrDefault() ?? "";
                    continue;
                case "tracestate":
                    tracestateValue = listVal?.Values?.FirstOrDefault() ?? "";
                    continue;
                case "grpc-trace-bin":
                    grpctracebinValue = listVal?.Values?.FirstOrDefault() ?? "";
                    continue;
                case "destination-app-id":
                    continue;
            }

            if (httpHeaderConversion && IsPermanentHTTPHeader(k))
            {
                keyName = DaprHeaderPrefix + keyName;
            }


            foreach (var v in listVal.Values)
            {
                var decodedString = v;
                if (k.EndsWith(GRPCBinaryMetadataSuffix))
                {
                    byte[] decoded = Convert.FromBase64String(v);
                    decodedString = Encoding.UTF8.GetString(decoded);
                }

                md.Add(keyName, decodedString);
            }

        }
        return md;
    }

    private static bool IsPermanentHTTPHeader(string k)
    {
        throw new NotImplementedException();
    }

    private static string ReservedGRPCMetadataToDaprPrefixHeader(string keyName)
    {
        throw new NotImplementedException();
    }

    private static void SetHeader(string v1, string v2)
    {
        throw new NotImplementedException();
    }

    public static MapField<string, ListStringValue> MetadataToInternalMetadata(Grpc.Core.Metadata headers)
    {
        var internalMD = new MapField<string, ListStringValue>();
        foreach (var item in headers)
        {
            var k = item.Key;
            var values = item.Value;
            if (k.EndsWith(GRPCBinaryMetadataSuffix))
            {
                byte[] decoded = Convert.FromBase64String(values);
                values = Encoding.UTF8.GetString(decoded);
            }
            var listStringValue = new ListStringValue();
            listStringValue.Values.Add(values);
            internalMD.Add(k, listStringValue);
        }

        return internalMD;
    }
}