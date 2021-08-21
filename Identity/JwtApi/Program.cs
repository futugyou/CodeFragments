using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JwtApi", Version = "v1" });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
     {
         options.Authority = "https://localhost:5001";
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

////'Code Flow'
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "Cookies";
//    options.DefaultChallengeScheme = "oidc";
//})
//    .AddCookie("Cookies")
//    .AddOpenIdConnect("oidc", options =>
//    {
//        options.Authority = "https://localhost:5001";
//        options.ClientId = "openidapi";
//        options.ClientSecret = "openidapi";
//        options.ResponseType = "code";
//        options.SaveTokens = true;
//        options.GetClaimsFromUserInfoEndpoint = true;
//        // mapping claimtype to jwttype
//        options.TokenValidationParameters.NameClaimType = JwtClaimTypes.Name;
//    });

//// 'Implicit Flow'
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultScheme = "Cookies";
//    options.DefaultChallengeScheme = "oidc";
//})
//    .AddCookie("Cookies")
//    .AddOpenIdConnect("oidc", options =>
//    {
//        options.Authority = "https://localhost:5001";
//        options.ClientId = "openidapi_implicit";
//        options.ClientSecret = "openidapi";
//        options.ResponseType = "id_token token";
//    });

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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtApi v1"));
}

app.UseHttpsRedirection();
app.UseCors("default");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
