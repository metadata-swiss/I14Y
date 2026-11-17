namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record IdentifierNameInputModel
{
    public string? Identifier { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
