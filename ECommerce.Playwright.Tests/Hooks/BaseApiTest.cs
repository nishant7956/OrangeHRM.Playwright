using Microsoft.Playwright;
using NUnit.Framework;
using ECommerce.Playwright.Framework.Config;

using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace ECommerce.Playwright.Tests.Hooks;

[TestFixture]
[AllureNUnit]
public abstract class BaseApiTest
{
    protected IAPIRequestContext Request { get; private set; } = null!;
    protected TestSettings Settings => TestSettings.Current;
    private IPlaywright _playwright = null!;

    [OneTimeSetUp]
    public async Task GlobalApiSetup()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Request = await _playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = Settings.ApiBaseUrl
        });
    }

    [OneTimeTearDown]
    public async Task GlobalApiTeardown()
    {
        if (Request != null)
        {
            await Request.DisposeAsync();
        }
        if (_playwright != null)
        {
            _playwright.Dispose();
        }
    }
}
