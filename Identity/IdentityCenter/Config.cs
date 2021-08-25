
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityCenter;
public static class Config
{
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("user_manage","user_manage"){
                Scopes = { "user_read", "user_edit" }
            },
        };

    public static IEnumerable<ApiScope> ApiScopes =>
           new List<ApiScope>
           {
               new ApiScope("api1", "My API"),
               new ApiScope("user_read", "user_read"),
               new ApiScope("user_edit", "user_edit"),
           };

    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "client",
                // no interactive user, use the clientid/secret for authentication
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // secret for authentication
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                // scopes that client has access to
                AllowedScopes = { "api1" }
            },
            new Client
            {
                ClientId = "openidapi",
                AllowedGrantTypes = GrantTypes.Code,
                ClientSecrets =
                {
                    new Secret("openidapi".Sha256())
                },
                // "profile" and "openid" IS MUST
                // IdentityServerConstants.StandardScopes.OpenId,
                // IdentityServerConstants.StandardScopes.Profile
                AllowedScopes = { "api1", "profile", "openid" },
                RedirectUris = { "https://localhost:5007/signin-oidc" },
                AllowOfflineAccess = true,
            },
            new Client
            {
                ClientId = "openidapi_implicit",
                AllowedGrantTypes = GrantTypes.Implicit,
                ClientSecrets =
                {
                    new Secret("openidapi".Sha256())
                },
                AllowedScopes = { "api1", IdentityServerConstants.StandardScopes.OpenId,  IdentityServerConstants.StandardScopes.Profile },
                RedirectUris = { "https://localhost:5009/signin-oidc" },
                AllowAccessTokensViaBrowser = true,
            },
            // JavaScript client
            new Client
            {
                ClientId = "spa",
                ClientName = "spa client",
                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                AllowedScopes = { "api1", IdentityServerConstants.StandardScopes.OpenId,  IdentityServerConstants.StandardScopes.Profile },
                RedirectUris = { "https://localhost:5005/html/callback.html" },
                PostLogoutRedirectUris = { "https://localhost:5005/html/index.html" },
                AllowedCorsOrigins = { "https://localhost:5005" },
            },
            // JavaScript BFF client
            new Client
            {
                ClientId = "bff",
                ClientSecrets = { new Secret("secret".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,
    
                // where to redirect to after login
                RedirectUris = { "https://localhost:5011/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5011/signout-callback-oidc" },

                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "api1"
                }
            }

        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
}
