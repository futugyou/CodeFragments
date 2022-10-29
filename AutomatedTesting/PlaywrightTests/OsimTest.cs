using Microsoft.Playwright.MSTest;

namespace PlaywrightTests;

[TestClass]
internal class OsimTest : PageTest
{
    // bin/Debug/net7.0/playwright.ps1 codegen https://devtest.oneusercenter.osim-cloud.com/
    [TestInitialize]
    public async Task TestInitialize()
    {
        await Page.GotoAsync("https://playwright.dev");
    }

    [TestMethod]
    public async Task MyTest()
    {
        await Page.GetByPlaceholder("Email Address").ClickAsync();
        await Page.GetByPlaceholder("Email Address").PressAsync("Control+a");
        await Page.GetByPlaceholder("Email Address").FillAsync("********@osim.com.cn");
        await Page.GetByText("Get OTP").ClickAsync();
        await Page.GetByPlaceholder("One-time Password").ClickAsync();
        await Page.GetByPlaceholder("One-time Password").FillAsync("212051");
        await Page.GetByText("Sign In").ClickAsync();
        await Page.WaitForURLAsync("https://devtest.oneusercenter.osim-cloud.com/feedback");
    }
}
