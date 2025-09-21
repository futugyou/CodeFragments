
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace KaleidoCode.Auth;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthExtension(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AuthOptions>(configuration.GetSection("AuthOptions"));
        var sp = services.BuildServiceProvider();
        var config = sp.GetRequiredService<IOptionsMonitor<AuthOptions>>()!.CurrentValue;

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
            .AddPolicy("AtLeast21", policy => policy.Requirements.Add(new MinimumAgeRequirement(21)))
            .AddPolicy("HasCountry", policy => policy.RequireAssertion(context => context.User.HasClaim(c => c.Type == ClaimTypes.Country)));

        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

        return services;
    }
}
