using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace PlaywrightSourceGenerator;
[Generator]
public class PlaywrightTestTemplate : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // begin creating the source we'll inject into the users compilation
        StringBuilder sourceBuilder = new StringBuilder(@"
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace RecordTraceTest;

[TestClass]
public class UserProfileGeneratorTest: PlaywrightTest
{
    [TestMethod]
    public async Task GetUserProfile()
    {
        var request = await Playwright.APIRequest.NewContextAsync();
        var data = new Dictionary<string, object>() {
          { ""password"", ""***!""},
          { ""userName"", ""***.cn"" },
          { ""phoneCode"", ""***"" }
        };

        var response = await request.PostAsync(""https://devtest.services.osim-cloud.com/identity/api/v1.0/account/login"", new() { DataObject = data });
        var headers = new Dictionary<string, string>();
        var body = await response.JsonAsync();
        var token = body.Value.GetProperty(""accessToken"").GetString();

        headers.Add(""Accept"", ""application/json"");
        // Add authorization token to all requests.
        // Assuming personal access token available in the environment.
        headers.Add(""Authorization"", ""Bearer "" + token);

        request = await Playwright.APIRequest.NewContextAsync(new()
        {
            // All requests we send go to this API endpoint.
            BaseURL = ""https://devtest.services.osim-cloud.com"",
            ExtraHTTPHeaders = headers,
        });

        var userReponse = await request.GetAsync(""/user/api/v1.0/userprofile"");
        var userbody = await userReponse.JsonAsync();
        var resultCode = userbody.Value.GetProperty(""resultCode"").GetInt32();
        var httpStatus = userbody.Value.GetProperty(""httpStatus"").GetInt32();

        // Act

        // Assert
        Assert.IsTrue(userReponse.Ok);
        Assert.IsTrue(resultCode == 1);
        Assert.IsTrue(httpStatus == 200);
    }
}

");

        // inject the created source into the users compilation
        context.AddSource("UserProfileGeneratorTest.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    public void Initialize(GeneratorInitializationContext context)
    {
    }
}