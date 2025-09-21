
namespace KaleidoCode.Auth;

public class AuthOptions
{
    public string JwksUri { get; set; }
    public string ValidIssuer { get; set; }
    public string ValidAudience { get; set; }
    public bool ValidateIssuer { get; set; } = false;
    public bool ValidateAudience { get; set; } = false;
}
