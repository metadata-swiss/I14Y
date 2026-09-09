namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record MappingRelationUriModel
{
    public required string Uri { get; init; }

    public string? Code { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
