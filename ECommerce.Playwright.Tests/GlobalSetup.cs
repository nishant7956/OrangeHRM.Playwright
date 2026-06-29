using NUnit.Framework;
using ECommerce.Playwright.Framework.Browser;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        await BrowserFactory.InitAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAllTests()
    {
        await BrowserFactory.DisposeAsync();
    }
}
