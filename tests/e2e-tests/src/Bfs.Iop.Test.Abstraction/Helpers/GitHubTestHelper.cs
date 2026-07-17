using Microsoft.Playwright;

namespace Bfs.Iop.Test.Abstraction.Helpers;

public static class GitHubTestHelper
{
    public static readonly string ResultsDirectory;

    static GitHubTestHelper()
    {
        ResultsDirectory = Environment.GetEnvironmentVariable("RESULTS_DIR") ?? "TestResults";

        Directory.CreateDirectory(ResultsDirectory);
    }

    public static async Task TakeAScreenShot(
        IPage page,
        string fileName = "",
        bool fullPage = true,
        ScreenshotType screenshotType = ScreenshotType.Png)
    {
        var generatedFileName = await GenerateFileName(page);

        var generatedFileNameWithExtension = 
            $"{(string.IsNullOrWhiteSpace(fileName) ? generatedFileName : fileName)}.{screenshotType.ToString().ToLowerInvariant()}";

        var targetPath =
            Path.Combine(
                ResultsDirectory,
                generatedFileNameWithExtension
                );

        await page.ScreenshotAsync(
            new()
            {
                Path = targetPath,
                FullPage = fullPage,
                Type = screenshotType
            });

        TestContext.AddTestAttachment(targetPath);
    }

    public static async Task SaveHtmlContent(
        IPage page,
        string fileName = "")
    {
        var generatedFileName = $"{await GenerateFileName(page)}.html";

        var targetPath =
            Path.Combine(
                ResultsDirectory,
                string.IsNullOrWhiteSpace(fileName) ? generatedFileName : fileName);

        await File.WriteAllTextAsync(
            Path.Combine(
                ResultsDirectory,
                string.IsNullOrWhiteSpace(fileName) ? generatedFileName : fileName),
            await page.ContentAsync());

        TestContext.AddTestAttachment(targetPath);
    }

    public static async Task PageDump(IPage page)
    {
        await TakeAScreenShot(page);
        await SaveHtmlContent(page);
    }

    private async static Task<string> GenerateFileName(IPage page)
    {
        var testName = SanitizeString(TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name);
        var pageTitle = SanitizeString(await page.TitleAsync());
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

        return $"{testName}_{pageTitle}_{timestamp}";
    }

    private static string SanitizeString(string value)
    {
        if (value.Length > 80)
        {
            value = value[..80];
        }

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidCharacter, '_');
        }

        return value.Replace(' ', '_');
    }
}