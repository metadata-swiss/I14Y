namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record MappingTableUriModel
{
    public required string Uri { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
