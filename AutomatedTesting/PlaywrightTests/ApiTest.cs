using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PlaywrightTests;

[TestClass]
public class ApiTest : PlaywrightTest
{
    // dotnet test --filter "ApiTest"
    // $env:password=*
    // $env:userName=*
    // $env:phoneCode=*
    // $env:PWDEBUG=1
    static string password = Environment.GetEnvironmentVariable("password")!;
    static string userName = Environment.GetEnvironmentVariable("userName")!;
    static string phoneCode = Environment.GetEnvironmentVariable("phoneCode")!;

    private IAPIRequestContext Request = null;

    [TestInitialize]
    public async Task SetUpAPITesting()
    {
        await CreateAPIRequestContext();
    }

    private async Task CreateAPIRequestContext()
    {
        var request = await this.Playwright.APIRequest.NewContextAsync();
        var data = new Dictionary<string, object>() {
          { "password", password},
          { "userName", userName },
          { "phoneCode", phoneCode }
        };

        var response = await request.PostAsync("https://devtest.services.osim-cloud.com/identity/api/v1.0/account/login", new() { DataObject = data });
        var headers = new Dictionary<string, string>();
        var body = await response.JsonAsync();
        var token = body.Value.GetProperty("accessToken").GetString();

        headers.Add("Accept", "application/json");
        // Add authorization token to all requests.
        // Assuming personal access token available in the environment.
        headers.Add("Authorization", "Bearer " + token);

        Request = await this.Playwright.APIRequest.NewContextAsync(new()
        {
            // All requests we send go to this API endpoint.
            BaseURL = "https://devtest.services.osim-cloud.com",
            ExtraHTTPHeaders = headers,
        });
    }

    [TestMethod]
    public async Task UserProfileTest()
    {
        // Arrange
        var userReponse = await Request.GetAsync("/user/api/v1.0/userprofile");
        var userbody = await userReponse.JsonAsync();
        var resultCode = userbody.Value.GetProperty("resultCode").GetInt32();
        var httpStatus = userbody.Value.GetProperty("httpStatus").GetInt32();

        // Act

        // Assert
        Assert.IsTrue(userReponse.Ok);
        Assert.IsTrue(resultCode == 1);
        Assert.IsTrue(httpStatus == 200);
    }
    [TestCleanup]
    public async Task TearDownAPITesting()
    {
        await Request.DisposeAsync();
    }
}
