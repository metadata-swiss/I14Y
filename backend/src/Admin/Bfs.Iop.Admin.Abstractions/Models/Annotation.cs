using System;

namespace Bfs.Iop.Admin.Models;

public class Annotation : IAnnotationInputModel
{
    public Guid Id { get; set; }

    public string? Identifier { get; set; }

    public MultiLanguage? Text { get; set; }

    public int Position { get; set; }

    public string? Title { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Uri { get; set; }
}