
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Microsoft.Extensions.DependencyInjection;

public static class AuthorizationServiceCollectionExtension
{
    public static IServiceCollection AddAuthorizationExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection("AuthOptions"));
        var config = configuration.GetSection("AuthOptions").Get<AuthOptions>() ?? new();

        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            config.JwksUri,
            new OpenIdConnectConfigurationRetriever()
        );

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = config.ValidateIssuer,
                    ValidateAudience = config.ValidateAudience,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config.ValidIssuer,
                    ValidAudience = config.ValidAudience,
                };

                options.ConfigurationManager = configManager;
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy("AtLeast18", policy => policy.Requirements.Add(new MinimumAgeRequirement(18)))
            .AddPolicy("HasEmail", policy => policy.RequireAssertion(context => context.User.HasClaim(c => c.Type == ClaimTypes.Email)));

        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        return services;
    }

}