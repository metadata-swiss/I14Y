namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DataServiceInputModel
{
    public required CodeInputModel AccessRights { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<VCardModel> ContactPoints { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public IEnumerable<ResourceModel> EndpointDescriptions { get; init; } = [];

    public IEnumerable<ResourceModel> EndpointUrls { get; init; } = [];

    public IEnumerable<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; init; } = [];

    public IEnumerable<ResourceModel> LandingPages { get; init; } = [];

    public CodeInputModel? License { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public required IdentifierInputModel Publisher { get; init; }

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public IEnumerable<IdModel> ServesDatasets { get; init; } = [];

    public IEnumerable<CodeInputModel> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public string? Version { get; init; }

    public MultiLanguageModel? VersionNotes { get; init; }
}
