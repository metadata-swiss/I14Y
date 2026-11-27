namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record IdentifierNameModel
{
    public string? Identifier { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
