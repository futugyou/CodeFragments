using Microsoft.AspNetCore.Http;

namespace OAuthDemoWithDotnet7;

public interface IPageRenderer
{
    IResult RenderLoginPage(string? username = null, string? password = null, string? errormessage = null);
    IResult RednerHomePage(string username);
}

public class PageRenderer : IPageRenderer
{
    public IResult RednerHomePage(string username)
    {
        var html = @$"
        <html>
            <head><title>Index</title></head>
            <body>
            <h3> welcome {username} </h3>
            <a href='Account/Logout'>Sign out</a>
            </body>
        </html>
        ";
        return Results.Content(html, "text/html");
    }

    public IResult RenderLoginPage(string? username = null, string? password = null, string? errormessage = null)
    {
        var html = @$"
        <html>
            <head><title>Login</title></head>
            <body>
            <form method='post'>
            <input type='text' name='username' value='{username}'></input>
            <input type='password' name='password' value='{password}'></input>
            <input type='submit' value='sign in'></input>
            <p style='color:red;'>{errormessage}</p>
            </form>
            </body>
        </html>
        ";
        return Results.Content(html, "text/html");
    }
}