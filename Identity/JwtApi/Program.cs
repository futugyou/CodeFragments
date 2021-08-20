using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "JwtApi", Version = "v1" });
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
//     {
//         options.Authority = "https://localhost:5001";
//         options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//         {
//             ValidateAudience = false
//         };
//     });


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie("Cookies")
 // 'Code Flow'
 //.AddOpenIdConnect("oidc", options =>
 //{
 //    options.Authority = "https://localhost:5001";
 //    options.ClientId = "openidapi";
 //    options.ClientSecret = "openidapi";
 //    options.ResponseType = "code";
 //    options.SaveTokens = true;
 //    options.GetClaimsFromUserInfoEndpoint = true;
 //});

 // 'Implicit Flow'
 .AddOpenIdConnect("oidc", options =>
  {
      options.Authority = "https://localhost:5001";
      options.ClientId = "openidapi_implicit";
      options.ClientSecret = "openidapi";
      options.ResponseType = "id_token token"; 
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
