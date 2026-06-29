using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ECommerce.Playwright.Framework.Browser;
using ECommerce.Playwright.Framework.Config;

using Allure.NUnit;
using Allure.NUnit.Attributes;

namespace ECommerce.Playwright.Tests.Hooks;

// ─────────────────────────────────────────────────────────────────────────────
//  BaseUiTest
//  ==========
//  Base class for all UI tests. Handles the NUnit lifecycle and Playwright
//  object graph management.
//
//  Lifecycle:
//    [OneTimeSetUp]    Init Playwright & Browser (once per test run)
//    [SetUp]           Create IBrowserContext & IPage (once per test)
//                      Start Tracing (if configured)
//    [TearDown]        Stop & Save Tracing (if failed/always)
//                      Close context
//    [OneTimeTearDown] Dispose Browser & Playwright
// ─────────────────────────────────────────────────────────────────────────────

[TestFixture]
[AllureNUnit]
public abstract class BaseUiTest
{
    // These are initialized per-test in SetUp.
    // They are declared nullable to satisfy the compiler, but are guaranteed
    // to be non-null when a test method runs.
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    // Expose settings to inheriting test classes
    protected TestSettings Settings => TestSettings.Current;

    // ─────────────────────────────────────────────────────────────────────────
    //  Global Setup / Teardown
    // ─────────────────────────────────────────────────────────────────────────

    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        // Initializes the Playwright process and launches the browser (e.g. Chromium).
        // This is expensive (~50-100ms) so we only do it once per test assembly/process.
        await BrowserFactory.InitAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await BrowserFactory.DisposeAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Per-Test Setup / Teardown
    // ─────────────────────────────────────────────────────────────────────────

    [SetUp]
    public async Task TestSetup()
    {
        // 1. Create a fresh, isolated context for this test
        Context = await BrowserFactory.CreateContextAsync();

        // 2. Start tracing if configured (Always or OnFailure)
        if (Settings.TraceMode != TraceMode.Never)
        {
            await Context.Tracing.StartAsync(new()
            {
                Title = TestContext.CurrentContext.Test.Name,
                Screenshots = true,
                Snapshots = true,
                Sources = true
            });
        }

        // 3. Create the tab (IPage) the test will interact with
        Page = await Context.NewPageAsync();
        
        TestContext.Out.WriteLine($"[SETUP] Started test: {TestContext.CurrentContext.Test.FullName}");
    }

    [TearDown]
    public async Task TestTeardown()
    {
        var result = TestContext.CurrentContext.Result;
        var status = result.Outcome.Status;
        bool isFailure = status == TestStatus.Failed;

        TestContext.Out.WriteLine($"[TEARDOWN] Test finished with status: {status}");

        try
        {
            // 1. Capture Screenshot on failure
            if (isFailure && Page != null)
            {
                Directory.CreateDirectory(Settings.ArtifactsDirectory);
                string safeTestName = string.Join("_", TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));
                string screenshotPath = Path.Combine(Settings.ArtifactsDirectory, $"{safeTestName}_fail.png");
                await Page.ScreenshotAsync(new() { Path = screenshotPath });
                TestContext.AddTestAttachment(screenshotPath, "Failure Screenshot");
            }

            // 2. Handle tracing
            if (Settings.TraceMode != TraceMode.Never)
            {
                bool shouldSaveTrace = Settings.TraceMode == TraceMode.Always || 
                                      (Settings.TraceMode == TraceMode.OnFailure && isFailure);

                if (shouldSaveTrace)
                {
                    Directory.CreateDirectory(Settings.ArtifactsDirectory);
                    string safeTestName = string.Join("_", TestContext.CurrentContext.Test.Name.Split(Path.GetInvalidFileNameChars()));
                    string traceFileName = $"{safeTestName}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
                    string tracePath = Path.Combine(Settings.ArtifactsDirectory, traceFileName);

                    await Context.Tracing.StopAsync(new() { Path = tracePath });
                    TestContext.AddTestAttachment(tracePath, "Playwright Trace");
                    TestContext.Out.WriteLine($"[TRACE] Saved trace to: {tracePath}");
                }
                else
                {
                    await Context.Tracing.StopAsync();
                }
            }
        }
        finally
        {
            string? videoPath = null;
            if (Settings.RecordVideo && Page?.Video != null)
            {
                videoPath = await Page.Video.PathAsync();
            }

            // 3. Always close the context to clean up resources.
            if (Context != null)
            {
                await Context.CloseAsync();
            }

            // 4. Attach video if recorded
            if (videoPath != null && File.Exists(videoPath))
            {
                TestContext.AddTestAttachment(videoPath, "Test Video");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Common Test Workflows
    // ─────────────────────────────────────────────────────────────────────────

}
