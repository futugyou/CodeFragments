
namespace OAuthDemoWithDotnet7;

public interface IAccountService
{
    bool Validate(string username, string password);
}

public class AccountService : IAccountService
{
    private readonly Dictionary<string, string> _accounts = new(StringComparer.OrdinalIgnoreCase)
    {
        {"one", "password"},
        {"two", "password"},
        {"three", "password"},
    };
    public bool Validate(string username, string password)
    {
        return _accounts.TryGetValue(username, out var pas) && pas == password;
    }
}