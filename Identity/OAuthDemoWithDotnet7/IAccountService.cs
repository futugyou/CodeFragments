
namespace OAuthDemoWithDotnet7;

public interface IAccountService
{
    bool Validate(string username, string password, out string[] roles);
}

public class AccountService : IAccountService
{
    private readonly Dictionary<string, string> _accounts = new (StringComparer.OrdinalIgnoreCase)
    {
        {"one", "password"},
        {"two", "password"},
        {"three", "password"},
    };

    private readonly Dictionary<string,string> _roles = new (StringComparer.OrdinalIgnoreCase)
    {
        {"one", "admin"}, 
    };

    public bool Validate(string username, string password, out string[] roles)
    {
        if (_accounts.TryGetValue(username, out var pas) && pas == password)
        {
            roles = _roles.TryGetValue(username, out var value)?value:Array.Empty<string>();
            return true;
        }
        roles = Array.Empty<string>();
        return false;
    }
}