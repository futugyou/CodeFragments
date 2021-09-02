using Microsoft.AspNetCore.Authentication.Cookies;

namespace OidcCodeApi.Extensions;
public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userPrincipal = context.Principal;
        if (userPrincipal != null)
        {
            // check claim
        }
    }
}