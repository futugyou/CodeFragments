using Microsoft.Playwright;
namespace RecordTraceTest;

public class UserProfile
{
    public static async Task GetUserProfile()
    {
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync();
        var data = new Dictionary<string, object>() {
          { "password", ""},
          { "userName", "" },
          { "phoneCode", "" }
        };

        var response = await request.PostAsync("https://devtest.services.osim-cloud.com/identity/api/v1.0/account/login", new() { DataObject = data });
        var headers = new Dictionary<string, string>();
        var body = await response.JsonAsync();
        var token = body.Value.GetProperty("accessToken").GetString();

        headers.Add("Accept", "application/json");
        // Add authorization token to all requests.
        // Assuming personal access token available in the environment.
        headers.Add("Authorization", "Bearer " + token);

        request = await playwright.APIRequest.NewContextAsync(new()
        {
            // All requests we send go to this API endpoint.
            BaseURL = "https://devtest.services.osim-cloud.com",
            ExtraHTTPHeaders = headers,
        });

        var userReponse = await request.GetAsync("/user/api/v1.0/userprofile");
        var userbody = await userReponse.JsonAsync();
        var resultCode = userbody.Value.GetProperty("resultCode").GetInt32();
        var httpStatus = userbody.Value.GetProperty("httpStatus").GetInt32();

        // Act

        // Assert
        Console.WriteLine(userReponse.Ok);
        Console.WriteLine(resultCode == 1);
        Console.WriteLine(httpStatus == 200);
    }
}
