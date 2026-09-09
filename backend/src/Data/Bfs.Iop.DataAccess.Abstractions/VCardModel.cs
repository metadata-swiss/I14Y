namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record VCardModel
{
    public MultiLanguageModel? Fn { get; init; }

    public MultiLanguageModel? HasAddress { get; init; }

    public required string HasEmail { get; init; }

    public string? HasTelephone { get; init; }

    public VCardKind Kind { get; init; } = VCardKind.Organization;

    public MultiLanguageModel? Note { get; init; }
}