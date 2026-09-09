namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DataServiceInputModel
{
    public required CodeInputModel AccessRights { get; init; }

    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public IReadOnlyCollection<VCardModel> ContactPoints { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IReadOnlyCollection<ResourceModel> Documentation { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> EndpointDescriptions { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> EndpointUrls { get; init; } = [];

    public IReadOnlyCollection<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IReadOnlyCollection<KeywordModel> Keywords { get; init; } = [];

    public IReadOnlyCollection<ResourceModel> LandingPages { get; init; } = [];

    public CodeInputModel? License { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public IReadOnlyCollection<IdModel> ServesDatasets { get; init; } = [];

    public IReadOnlyCollection<CodeInputModel> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
