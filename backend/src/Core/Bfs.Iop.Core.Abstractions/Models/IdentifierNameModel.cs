namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IdentifierNameModel
{
    public required string Identifier { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
