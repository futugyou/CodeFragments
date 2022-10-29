using Microsoft.Playwright;

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
await using var context = await browser.NewContextAsync();

// Start tracing before creating / navigating a page.
await context.Tracing.StartAsync(new()
{
    Screenshots = true,
    Snapshots = true,
    Sources = true
});

var page = await context.NewPageAsync();
await page.GotoAsync("https://devtest.oneusercenter.osim-cloud.com");

// Stop tracing and export it into a zip archive.
// bin/Debug/net7.0/playwright.ps1 show-trace trace.zip
// BUG: Trace file trace.zip does not exist!
await context.Tracing.StopAsync(new()
{
    Path = "trace.zip"
});
await page.ScreenshotAsync(new() { Path = "screenshot.png" });