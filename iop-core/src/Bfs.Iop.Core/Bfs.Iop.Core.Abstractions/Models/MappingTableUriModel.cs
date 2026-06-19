namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MappingTableUriModel
{
    public required string Uri { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
