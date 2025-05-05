namespace Config;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Protocol
{
    [JsonStringEnumMemberName("grpc")] GRPCProtocol = 0,
    [JsonStringEnumMemberName("grpcs")] GRPCSProtocol = 1,
    [JsonStringEnumMemberName("http")] HTTPProtocol = 2,
    [JsonStringEnumMemberName("https")] HTTPSProtocol = 3,
    [JsonStringEnumMemberName("h2c")] H2CProtocol = 4,
}
public static class ProtocolExtensions
{
    public static bool IsHTTP(this Protocol protocol)
    {
        return protocol == Protocol.HTTPProtocol ||
               protocol == Protocol.HTTPSProtocol ||
               protocol == Protocol.H2CProtocol;
    }

    public static bool HasTLS(this Protocol protocol)
    {
        return protocol == Protocol.HTTPSProtocol ||
               protocol == Protocol.GRPCSProtocol;
    }
}