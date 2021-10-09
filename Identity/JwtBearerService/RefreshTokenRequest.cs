using System.Text.Json.Serialization;

namespace JwtBearerService;

public class RefreshTokenRequest
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }
}