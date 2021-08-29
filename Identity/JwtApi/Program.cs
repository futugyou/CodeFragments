using IdentityModel;
using JwtApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JwtApi",
        Version = "v1",
        Description = "JwtApi API"
    });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows()
        {
            Implicit = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri($"https://localhost:5001/connect/authorize"),
                TokenUrl = new Uri($"https://localhost:5001/connect/token"),
                Scopes = new Dictionary<string, string>()
                {
                    { "api1", "JwtApi-Api1" }
                }
            }
        }
    });

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
     {
         options.Authority = "https://localhost:5001";
         // this is scope
         options.Audience = "api1";
         // if does not need https 
         //options.RequireHttpsMetadata = false;
         options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
         {
             ValidateAudience = false
         };
     });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "api1");
    });
});

// add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.WithOrigins("https://localhost:5005")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtApi v1");
        c.OAuthClientId("jwtapiswaggerui");
        c.OAuthAppName("JwtApi Swagger UI");
    });
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
