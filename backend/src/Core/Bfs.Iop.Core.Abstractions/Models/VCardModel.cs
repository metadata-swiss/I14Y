namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record VCardModel
{
    public MultiLanguageModel? Fn { get; init; }

    public MultiLanguageModel? HasAddress { get; init; }

    public required string HasEmail { get; init; }

    public string? HasTelephone { get; init; }

    public VCardKind Kind { get; init; }

    public MultiLanguageModel? Note { get; init; }
}