using Bfs.Iop.Test.Abstraction.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using NUnit.Framework.Interfaces;

namespace Bfs.Iop.Public.Testautomation.Helpers;

public class PlaywrightSetup : IDisposable
{
    public string BasePublicUrl { get; }
    public string CurrentDate { get; }
    public string TimeStamp { get; }

    public IPage Page => _page ?? throw new InvalidOperationException("Page is not initialized");

    private EventHandler<IFrame> _handlerFrameNavigated;
    private EventHandler<IRequest> _handlerRequest;
    private EventHandler<IRequest> _handlerRequestFailed;

    private Wrapper? _wrapper;

    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private readonly IConfiguration _configuration;
    private IPlaywright? _playwright;
    private readonly bool _isHeadless;
    private readonly bool _recordVideo = true; // Record a video 
    private readonly bool _recordAllTests = true; // true = record all tests, false = only failed tests
    private readonly bool _windowsMaximized = true;
    private readonly bool _traceTests;

    private async Task CreateBrowser()
    {
        _playwright = await Playwright.CreateAsync();
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _isHeadless,
            SlowMo = 50,
            Args = _windowsMaximized ? new[] { "--start-fullscreen" } : null
        };

        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            TimezoneId = "Europe/Zurich",
        };

        if (_recordVideo)
        {
            var videoDir = Path.Combine(Directory.GetCurrentDirectory(), "Videos");
            if (!Directory.Exists(videoDir))
            {
                Directory.CreateDirectory(videoDir);
                TestContext.Out.WriteLine($"Video directory created: {videoDir}");
            }

            TestContext.Out.WriteLine($"Using video directory: {videoDir}");
            TestContext.Out.WriteLine($"RecordVideo setting: {_recordVideo}");
            TestContext.Out.WriteLine($"RecordAllTests setting: {_recordAllTests}");

            contextOptions.RecordVideoDir = videoDir;
            contextOptions.RecordVideoSize = _windowsMaximized ? null : new RecordVideoSize
            {
                Width = 1920,
                Height = 1080
            };
        }

        _context = await _browser.NewContextAsync(contextOptions);
        _page = await _context.NewPageAsync();
    }

    public void Dispose()
    {
        OneTimeTearDown().GetAwaiter().GetResult();
    }

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        await CreateBrowser();              

        await Page.GotoAsync(BasePublicUrl);

        _wrapper = new Wrapper(Page);

        TestContext.Out.WriteLine("Browser started");
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        try
        {
            if (_traceTests)
            {
                await _context!.Tracing.StopAsync(new()
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

    public PlaywrightSetup()
    {
        try
        {
            // Load the configuration
            var environment =
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "DEV"; // sensible local default

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<PlaywrightSetup>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            BasePublicUrl = _configuration["PlaywrightConfig:BASE_PUBLIC_URL"] ?? throw new InvalidOperationException("Cannot read BASE_PUBLIC_URL.");

            _recordVideo = bool.Parse(_configuration["PlaywrightConfig:RecordVideo"] ?? "false");
            _recordAllTests = bool.Parse(_configuration["PlaywrightConfig:RecordAllTests"] ?? "false");
            _isHeadless = bool.Parse(_configuration["PlaywrightConfig:HeadLess"] ?? "true");
            _traceTests = bool.Parse(_configuration["PlaywrightConfig:TraceTests"] ?? "false");

            CurrentDate = DateTime.Now.ToString("dd.MM.yyyy");
            TimeStamp = DateTime.Now.Ticks.ToString();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error loading configuration: {ex.Message}");
        }
    }

    public Wrapper Actions
    {
        get { return _wrapper!; }
    }

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

        string? videoPath = null;
        string? targetPath = null;

        try
        {
            videoPath = await _page!.Video!.PathAsync();
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

                    try
                    {
                        File.Delete(videoPath);
                    }
                    catch
                    {
                        // Ignore if deletion fails
                    }

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
