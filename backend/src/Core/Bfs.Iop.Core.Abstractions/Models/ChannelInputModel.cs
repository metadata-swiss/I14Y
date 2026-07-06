namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record ChannelInputModel
{
    public MultiLanguageModel? Address { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? Email { get; init; }

    public string? Fax { get; init; }

    public Guid? Id { get; init; }

    public required string Identifier { get; init; }

    public string? Mobile { get; init; }

    public string? OpeningHours { get; init; }

    public IEnumerable<IdentifierInputModel> OwnedBy { get; init; } = [];

    public string? Phone { get; init; }

    public CodeInputModel? Type { get; init; }

    public string? Url { get; init; }
}
