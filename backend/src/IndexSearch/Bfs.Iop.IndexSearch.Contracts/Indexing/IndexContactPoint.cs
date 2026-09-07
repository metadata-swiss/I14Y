using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record IndexContactPoint
{
    public MultiLanguageModel? Fn { get; init; }

    public MultiLanguageModel? HasAddress { get; init; }

    public MultiLanguageModel? Note { get; init; }

    public string? HasEmail { get; init; }

    public string? HasTelephone { get; init; }
}