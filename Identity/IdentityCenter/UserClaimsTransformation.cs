using IdentityCenter.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityCenter;
public class UserClaimsTransformation : IClaimsTransformation
{
    /// <summary>
    /// add user claim, if it does not exists. 
    /// But, now i can not show it in Client.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var claimType = "card";
        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            var claim = new Claim(claimType, "aaaaa");
            foreach (var claimsIdentity in principal.Identities)
            {
                claimsIdentity.AddClaim(claim);
            }
        }
        return Task.FromResult(principal);
    }
}