using Microsoft.Playwright;

namespace Bfs.Iop.Test.Abstraction.Helpers;

// <summary>
/// Tracks and monitors URL changes in a Playwright page session.
/// Provides functionality to detect navigation changes and access both initial and current URLs.
/// </summary>
/// <remarks>
/// This class maintains the initial URL state and provides methods to check if the page URL 
/// has changed during test execution. It's particularly useful for validating navigation flows 
/// and page transitions in UI tests.
/// </remarks>
public class PageUrlTracker 
{
    private readonly string _initialUrl;
  

    public PageUrlTracker(IPage page)
    {
        _initialUrl = page.Url;
    }

    public bool HasChanged(IPage page) => page?.Url != _initialUrl;

    public string InitialUrl => _initialUrl;

    public string CurrentUrl(IPage page) => page.Url;

    public string GetChangedPath(IPage page)
    {
        if (!HasChanged(page))
            return string.Empty;

        return new Uri(page.Url).PathAndQuery;
    }

    public void Dispose()
    {
     
        GC.SuppressFinalize(this);
    }

    
}