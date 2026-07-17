using Bfs.Iop.Admin.Testautomation.Enums;
using Bfs.Iop.Test.Abstraction.Helpers;
using Bfs.Iop.Test.Abstraction.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;
using System.Drawing;

namespace Bfs.Iop.Admin.Testautomation.Helpers;

/// <summary>
/// Base class for Playwright UI tests:
/// loads configuration (URLs, credentials, video flags), initializes and manages browser/context/page,
/// performs login, optionally records videos, and cleans up resources at the end.
/// </summary>
public class PlaywrightSetup : IDisposable
{
    private Wrapper? _wrapper;
    private readonly Environments _environment;
    private bool _disposed;

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private IPlaywright? _playwright;
    private readonly bool _isHeadless;
    private readonly IConfiguration _configuration;
    private readonly bool _recordVideo;
    private readonly bool _recordAllTests;
    private readonly bool _windowsMaximized = true;
    private readonly bool _traceTests;

    private EventHandler<IFrame>? _handlerFrameNavigated;
    private EventHandler<IRequest>? _handlerRequest;
    private EventHandler<IRequest>? _handlerRequestFailed;

    public string BaseAdminUrl { get; }

    public string BaseIopCoreUrl { get; }

    public string BaseDatasetUriPrefix { get; }

    public string UserName { get; }

    public string Password { get; }

    public string Tan { get; }

    public string CurrentDate { get; }

    public string TimeStamp { get; }

    public IPage Page => _page ?? throw new InvalidOperationException("Page is not initialized");

    public Wrapper Actions => _wrapper!;

    public PlaywrightSetup()
    {
        try
        {
            // load environment
            var environment =
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "DEV"; // sensible default

            if (!Enum.TryParse(environment, true, out _environment))
            {
                _environment = Environments.DEV;
            }

            _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{environment}.json", optional: false, reloadOnChange: true)
                    .AddUserSecrets<PlaywrightSetup>(optional: true)
                    .AddEnvironmentVariables()
                    .Build();

            Password = _configuration["PlaywrightConfig:PASSWORD"] ?? throw new InvalidOperationException("The password must not be zero or empty."); //secrets
            Tan = _configuration["PlaywrightConfig:TAN"] ?? throw new InvalidOperationException("The tan must not be zero or empty.");  //secrets

            _recordVideo = bool.Parse(_configuration["PlaywrightConfig:RecordVideo"] ?? "false");
            _recordAllTests = bool.Parse(_configuration["PlaywrightConfig:RecordAllTests"] ?? "false");
            _isHeadless = bool.Parse(_configuration["PlaywrightConfig:HeadLess"] ?? "true");
            _traceTests = bool.Parse(_configuration["PlaywrightConfig:TraceTests"] ?? "false");

            BaseAdminUrl = _configuration[$"PlaywrightConfig:ADMIN_BASE_URL"] 
                ?? throw new InvalidOperationException("Cannot read ADMIN_BASE_URL.");

            BaseIopCoreUrl = _configuration[$"PlaywrightConfig:DCAT_BASE_URL"] 
                ?? throw new InvalidOperationException("Cannot read DCAT_BASE_URL.");

            UserName = _configuration["PlaywrightConfig:UserName"] 
                ?? throw new InvalidOperationException("Cannot read Username.");  //secrets

            BaseDatasetUriPrefix = _configuration["PlaywrightConfig:BaseDatasetUriPrefix"] 
                ?? throw new InvalidOperationException("Cannot read the base dataset uri prefix.");

            CurrentDate = DateTime.Now.ToString("dd.MM.yyyy");
            TimeStamp = DateTime.Now.Ticks.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading configuration: {ex.Message}");
        }
    }
    
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            CleanupResources().RunSynchronously();
        }
        catch (Exception)
        {
            // Already tidied up
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Sets Browser/Context/Page and performs a one-time login before all tests.
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        try
        {
            await CreateBrowser();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

        _wrapper = new Wrapper(Page);

        await BindEiamHack();

        await Actions.Wait1000();

        await Page.GotoAsync(BaseAdminUrl);

        await Actions.Wait1500();

        await Login();
    }

    // <summary>
    /// Saves recorded video in case of failure and then cleans up all resources.
    /// </summary>
    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        try
        {
            if (_traceTests && _context is not null)
            {
                await _context.Tracing.StopAsync(new()
                {
                    Path = Path.Combine(GitHubTestHelper.ResultsDirectory, "trace.zip")
                });
            }

            await SaveAndProcessVideo();
        }
        finally
        {
            await CleanupResources();
        }
    }

    private async Task Login()
    {
        TestContext.Out.WriteLine("start to login");
        var login = new EiamLogin();

        var result = await login.TryConnectToEiam(Page, UserName, Password, Tan);
        if (!result)
        {
            await Actions.Wait500();
            result = await login.TryConnectToEiam(Page, UserName, Password, Tan);
        }

        if (result)
        {
            await Actions.Wait3000();
            TestContext.Out.WriteLine("chose profile");
            result = await EiamLogin.ChooseProfile(Page, Constants.Eiam.OrganisationI14yTest, Constants.Eiam.RoleLocalDataSteward);

            // Wait 3.5 seconds for the page to stabilize
            await Actions.Wait3500();
        }

        TestContext.Out.WriteLine($"Recording enabled: {_recordVideo}");
        TestContext.Out.WriteLine($"Record all tests: {_recordAllTests}");
        TestContext.Out.WriteLine($"Current environment: {_environment}");

        Assert.That(result, Is.True, "Could not log in with Eiam");
    }

    /// <summary>
    /// Copies the WebM video to the project directory and appends it if tests fail.
    /// </summary>
    private async Task SaveAndProcessVideo()
    {
        if (!_recordVideo || (!_recordAllTests && TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed))
        {
            return;
        }

        if (_page == null || _page.Video == null)
        {
            return;
        }

        string? targetPath = null;
        string? videoPath;

        try
        {
            videoPath = await _page.Video.PathAsync();
            if (videoPath != null)
            {
                var testName = TestContext.CurrentContext.Test.Name;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var status = TestContext.CurrentContext.Result.Outcome.Status.ToString();
                var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\.."));
                var videoFolder = Path.Combine(projectRoot, "Videos");
                Directory.CreateDirectory(videoFolder);
                targetPath = Path.Combine(videoFolder, $"{testName}_{status}_{timestamp}.webm");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Error getting video path: {ex.Message}");
            return;
        }

        if (videoPath != null && targetPath != null && File.Exists(videoPath))
        {
            for (int retry = 0; retry < 5; retry++)
            {
                try
                {
                    await Task.Delay(5000);

                    using (var sourceStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var destinationStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }

                    TestContext.AddTestAttachment(targetPath);

                    break;
                }
                catch (IOException) when (retry < 4)
                {
                    await Task.Delay(5000);
                    TestContext.WriteLine($"Retry {retry + 1} of 5 copying video file");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Error copying video file: {ex.Message}");
                    break;
                }
            }
        }
    }

    // <summary>
    /// Safely closes and disposes the Playwright page, context, and browser.
    /// </summary>
    private async Task CleanupResources()
    {
        try
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }

            if (_context != null)
            {
                await _context.CloseAsync();
                await _context.DisposeAsync();
                _context = null;
            }

            if (_browser != null)
            {
                await _browser.CloseAsync();
                await _browser.DisposeAsync();
                _browser = null;
            }

            if (_playwright != null)
            {
                _playwright.Dispose();
                _playwright = null;
            }

            _wrapper = null;
            TestContext.WriteLine("Browser cleanup completed");
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Cleanup error: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates and configures the Playwright browser, context, and page, including video options.
    /// </summary>
    private async Task CreateBrowser()
    {
        var windowSize = new Size(1920, 1080);
        _playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _isHeadless,
            SlowMo = 50,
            Args = null,
        };

        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = windowSize.Width,
                Height = windowSize.Height
            },
            TimezoneId = "Europe/Zurich",
            AcceptDownloads = true
        };

        if (_recordVideo)
        {
            contextOptions.RecordVideoDir = GitHubTestHelper.ResultsDirectory;
            contextOptions.RecordVideoSize = _windowsMaximized ? null : new RecordVideoSize
            {
                Width = windowSize.Width,
                Height = windowSize.Height
            };
        }

        _context = await _browser.NewContextAsync(contextOptions);

        if (_traceTests)
        {
            await _context.Tracing.StartAsync(new TracingStartOptions()
            {
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }        

        _page = await _context.NewPageAsync();
    }

    private async Task BindEiamHack()
    {
        TestContext.Out.WriteLine("bind Eiam hack");

        await Page.GotoAsync(Test.Abstraction.Constants.Eiam.UrlAutoLogOn);

        await Actions.Wait500();

        var autoLogOnToggleId = _environment == Environments.ABN
          ? Test.Abstraction.Constants.Eiam.ToggleAutoLogOnAbn
          : Test.Abstraction.Constants.Eiam.ToggleAutoLogOnRef;

        await Actions.ScrollIntoViewById(autoLogOnToggleId);

        var slideToggle = await Actions.FindElementById(autoLogOnToggleId);

        if (slideToggle != null)
        {
            await Actions.ClickCheckBoxById(autoLogOnToggleId);
        }
        else
        {
            TestContext.Out.WriteLine("Unabled to use Eiam hack");
            Assert.Fail("Unabled to use Eiam hack");
        }
    }

    [SetUp]
    public void SetupNavigationLogging()
    {
        var outWriter = TestContext.Out;

        outWriter.WriteLine($"Current Page Url: {Page.Url}");

        _handlerFrameNavigated = (_, frame) =>
        {
            if (frame.ParentFrame == null) // main frame only
            {
                outWriter.WriteLine($"[NAV] {frame.Url}");
            }
        };

        _handlerRequest = (_, request) =>
        {
            if (request.IsNavigationRequest)
            {
                outWriter.WriteLine($"[NAV-REQ] {request.Method} {request.Url}");
            }
        };

        _handlerRequestFailed = (_, request) =>
        {
            if (request.IsNavigationRequest)
            {
                outWriter.WriteLine($"[NAV-FAIL] {request.Method} {request.Url} :: {request.Failure}");
            }
        };

        Page.FrameNavigated += _handlerFrameNavigated;
        Page.Request += _handlerRequest;
        Page.RequestFailed += _handlerRequestFailed;
    }

    [TearDown]
    public void TearDownNavigationLogging()
    {
        Page.FrameNavigated -= _handlerFrameNavigated;
        Page.Request -= _handlerRequest;
        Page.RequestFailed -= _handlerRequestFailed;
    }
}