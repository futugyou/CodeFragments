using Microsoft.Playwright.MSTest;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaywrightTests;

[TestClass]
internal class BrowserContextTest : PageTest
{
    [TestMethod]
    public async Task TestWithCustomContextOptions()
    {
        // The following Page (and BrowserContext) instance has the custom colorScheme, viewport and baseURL set:
        await Page.GotoAsync("/login");
    }

    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
            ColorScheme = ColorScheme.Light,
            ViewportSize = new()
            {
                Width = 1920,
                Height = 1080
            },
            BaseURL = "https://github.com",
        };
    }
}