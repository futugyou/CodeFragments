using OAuthDemoWithDotnet7;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddSingleton<IPageRenderer, PageRenderer>();
builder.Services.AddSingleton<IAccountService, AccountService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

//builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();
app.Map("/", WelcomeAsync);
app.MapGet("Account/Login", Login);
app.MapPost("Account/Login", SignInAsync);
app.Map("Account/Logout", SignOutAsync);
app.Map("Account/AccessDenied", DeniedAccess);
app.Run();

Task WelcomeAsync(HttpContext context, ClaimsPrincipal user, IPageRenderer pageRender,IAuthorizationService authService)
{
    if (user?.Identity?.IsAuthenticated)
    {
        var requirement = new RolesAuthorizationRequirement(new string[]{"admin"});
        var result = await authService.AuthorizeAsync(user:user,resource:null,requriement:new IAuthorizationRequriement[]{requriement});
        if(result.Succeeded)
        {
            await  pageRender.RednerHomePage(user.Identity.Name!).ExecuteAsync(context);
        }
        else
        {
            await context.ForbidAsync();   
        }
    }

    await context.ChallengeAsync();
}

IResult Login(IPageRenderer radner)
{
    return radner.RenderLoginPage();
}

Task SignInAsync(HttpContext context, HttpRequest request, IPageRenderer pageRender, IAccountService accountService)
{
    var username = request.Form("username");
    if (string.IsNullOrEmpty(username))
    {
        return radner.RenderLoginPage(null,null,"enter name").ExecuteAsync(context);
    }

    var password = request.Form("password");
    if (string.IsNullOrEmpty(password))
    {
        return radner.RenderLoginPage(null,null,"enter password").ExecuteAsync(context);
    }

    if (!accountService.Validate(username,password,out var roles))
    {
        return radner.RenderLoginPage(username,null,"invalid name or password").ExecuteAsync(context);
    }

    var identity = new GenericIdentity(name: username, type:"PASSWORD");
    foreach (var role in roles)
    {
        identity.AddClaim(new Claim(ClaimTypes.Role,role));
    }
    var user = new ClaimsPrincipal(identity);
    return context.SignInAsync(user);
}

async Task SignOutAsync(HttpContext context)
{
    await context.SignOutAsync();
    context.Response.Redirect("/");
}
IResult DeniedAccess(ClaimsPrincipal user, IPageRenderer render)
{
    return render.RenderAccessDeniedPage(user?.Identity?.Name);
}