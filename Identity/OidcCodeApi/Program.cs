using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OidcCodeApi.Extensions;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<CustomCookieAuthenticationEvents>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "OidcCodeApi", Version = "v1" });
});

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
//'Code Flow'
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies", options =>
    {
        options.EventsType = typeof(CustomCookieAuthenticationEvents);
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";
        options.ClientId = "openidapi";
        options.ClientSecret = "openidapi";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "card",
            // RoleClaimType = "role"
        };
        options.Scope.Add("profile");
        options.Scope.Add("api1");
        options.Scope.Add("offline_access");
        options.Scope.Add("card");
        options.ClaimActions.MapUniqueJsonKey("card", "card");
        // mapping claimtype to jwttype
        options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OidcCodeApi v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
