
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using static Duende.IdentityServer.IdentityServerConstants;

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
                // Client cannot request a refresh token in client credentials flow
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                // secret for authentication
                ClientSecrets =
                {
                    //new Secret("secret".Sha256()),
                    // name based
                    //new Secret(@"CN=clientone, OU=clientone, O=cxagroup", "client.dn")
                    //{
                    //    Type = SecretTypes.X509CertificateName
                    //},

                    // or thumbprint based
                    new Secret("13EABFEAE46BC2E93BDB85BC05BF64AC1C74A475")
                    {
                        Type = SecretTypes.X509CertificateThumbprint
                    },
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
                // Refreshing Token
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
                AlwaysIncludeUserClaimsInIdToken = true,
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
            },
            new Client
            {
                ClientId = "jwtapiswaggerui",
                ClientName = "JwtApi Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = { $"https://localhost:5003/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"https://localhost:5003/swagger/" },
                AllowedScopes =
                {
                    "api1"
                }
            },
        };

    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
}
