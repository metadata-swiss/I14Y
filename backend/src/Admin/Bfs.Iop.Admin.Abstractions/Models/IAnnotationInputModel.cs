namespace Bfs.Iop.Admin.Models;

public interface IAnnotationInputModel
{
    public string? Identifier { get; }

    public MultiLanguage? Text { get; }

    public string? Title { get; }

    public string Type { get; }

    public string? Uri { get; }
}
