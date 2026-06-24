namespace Bfs.Iop.Core.Abstractions.Models;

public record ExportFile(Stream Data, string FileName, string MimeType) : IDisposable
{
    private bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Data?.Dispose();
        }

        _disposed = true;
    }
}
