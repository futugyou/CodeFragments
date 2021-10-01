namespace JwtBearerService;

public class JwtSettings
{
    public string SecurityKey { get; set; }
    public TimeSpan ExpiresIn { get; set; }
}