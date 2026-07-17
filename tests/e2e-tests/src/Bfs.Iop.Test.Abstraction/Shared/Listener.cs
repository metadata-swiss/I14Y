using Microsoft.Playwright;

namespace Bfs.Iop.Test.Abstraction.Shared;

/// <summary>
/// Monitors and handles API responses in Playwright tests, providing error detection and timeout management.
/// </summary>
public sealed class Listener : IDisposable
{
    private readonly IPage _page;
    private bool _hasApiError = false;
    private string _lastErrorMessage = string.Empty;
    private TaskCompletionSource<bool> _responseHandled;
    private EventHandler<IResponse>? _responseHandler;
    private bool _disposed;

    public Listener(IPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _responseHandled = new TaskCompletionSource<bool>();
    }

    public void RecognizeApiErrors()
    {
        _responseHandler = async (_, response) =>
        {
            try
            {
                if ((response.Status >= 400 && response.Status < 600))
                {
                    _hasApiError = true;
                    _lastErrorMessage = $"API Error detected: {response.Status} for {response.Url}: {await response.TextAsync()}";
                }
            }
            catch (Exception ex)
            {
                _hasApiError = true;
                _lastErrorMessage = $"Error processing response: {ex.Message}";
            }
            finally
            {
                _responseHandled.TrySetResult(true);
            }
        };

        _page.Response += _responseHandler;
    }

    public async Task WaitForResponseHandlingAsync()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var completedTask = await Task.WhenAny(
                _responseHandled.Task,
                Task.Delay(5000, cts.Token)
            );

            if (completedTask != _responseHandled.Task)
            {
                _responseHandled.TrySetResult(true);
                TestContext.WriteLine("Response handling timed out after 5 seconds");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Error waiting for response handling: {ex.Message}");
            _responseHandled.TrySetResult(true);
            throw;
        }
    }

    public bool HasApiErrors() => _hasApiError;

    public string GetLastErrorMessage() => _lastErrorMessage;

    public void ResetErrors()
    {
        _hasApiError = false;
        _lastErrorMessage = string.Empty;
        _responseHandled = new TaskCompletionSource<bool>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_responseHandler != null)
        {
            _page.Response -= _responseHandler;
            _responseHandler = null;
        }

        GC.SuppressFinalize(this);
    }
}
