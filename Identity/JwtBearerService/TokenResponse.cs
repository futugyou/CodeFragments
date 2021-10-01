using System.Text.Json.Serialization;

namespace JwtBearerService;
public class TokenResponse
{
    [JsonPropertyName("access_token")] 
    public string AccessToken { get; set; }
    [JsonPropertyName("token_type")] 
    public string TokenType { get; set; }
}