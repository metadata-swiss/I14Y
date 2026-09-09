namespace Bfs.Iop.Core.Lucene.Filters;

internal sealed record AnnotationFilter
{
    public required string FilteredField { get; init; }

    public required List<string> Values { get; init; }
}