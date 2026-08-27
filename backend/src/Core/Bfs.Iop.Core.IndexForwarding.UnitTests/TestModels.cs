using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.IndexForwarding.UnitTests;

/// <summary>
/// Fully-populated domain models for projection tests.
/// <para>
/// Deliberately fills every field the index stores, including the optional ones. A fixture that
/// leaves fields null cannot tell "the projection does not map this" from "the test did not set it",
/// which is exactly the gap <see cref="IndexEntryProjectionShapeTests"/> exists to close.
/// </para>
/// </summary>
internal static class TestModels
{
    private static readonly SystemInfoModel System = new()
    {
        CreatedAt = DateTimeOffset.UnixEpoch,
        ModifiedAt = DateTimeOffset.UnixEpoch.AddDays(1),
        CreationType = Abstractions.Models.CreationType.Manual,
    };

    private static MultiLanguageModel Text(string value) => new() { De = value };

    private static AgentModel Publisher() => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH_BFS",
        Name = Text("Bundesamt fuer Statistik"),
        PrefLabel = Text("BFS"),
        System = System,
    };

    private static VocabularyEntryModel Vocab(string code) => new() { Code = code };

    private static IopPersonModel Person() => new()
    {
        GivenName = "Ada",
        FamilyName = "Lovelace",
        Email = "ada@example.ch",
    };

    private static VCardModel ContactPoint() => new()
    {
        Fn = Text("Kontakt"),
        HasAddress = Text("Bern"),
        Note = Text("Notiz"),
        HasEmail = "kontakt@example.ch",
    };

    private static IEnumerable<KeywordModel> Keywords() => [new KeywordModel { Label = Text("stichwort") }];

    public static DcatDatasetModel Dataset() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["ds-1"],
        Publisher = Publisher(),
        System = System,
        PublicationLevel = PublicationLevel.Public,
        PublicationLevelProposal = PublicationLevel.Internal,
        RegistrationStatus = RegistrationStatus.Recorded,
        RegistrationStatusProposal = RegistrationStatus.Qualified,
        Title = Text("Titel"),
        Description = Text("Beschreibung"),
        Keywords = Keywords(),
        Version = "1.0.0",
        AccessRights = Vocab("PUBLIC"),
        Themes = [Vocab("ENER")],
        DataOwner = "BFS",
        ResponsiblePerson = Person(),
        ResponsibleDeputy = Person(),
        ContactPoints = [ContactPoint()],
        Distributions = [new DcatDistributionModel { Format = Vocab("CSV") }],
    };

    public static DataServiceModel DataService() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["dsv-1"],
        Publisher = Publisher(),
        System = System,
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Recorded,
        Title = Text("Titel"),
        Description = Text("Beschreibung"),
        Keywords = Keywords(),
        Version = "1.0.0",
        AccessRights = Vocab("PUBLIC"),
        Themes = [Vocab("ENER")],
        ResponsiblePerson = Person(),
        ResponsibleDeputy = Person(),
        ContactPoints = [ContactPoint()],
    };

    public static PublicServiceModel PublicService() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["ps-1"],
        Publisher = Publisher(),
        System = System,
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Recorded,
        Name = Text("Name"),
        Description = Text("Beschreibung"),
        Keywords = Keywords(),
        ThematicAreas = [Vocab("AREA")],
        Sectors = [Vocab("SECTOR")],
        BusinessEvents = [Vocab("BE")],
        LifeEvents = [Vocab("LE")],
        ResponsiblePerson = Person(),
        ResponsibleDeputy = Person(),
        Channels = [new ChannelModel { Id = Guid.NewGuid(), Email = "kanal@example.ch", Identifier = "ch-1" }],
    };

    public static IopConceptModel Concept() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["concept-1"],
        Publisher = Publisher(),
        System = System,
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Recorded,
        Name = Text("Name"),
        Description = Text("Beschreibung"),
        Keywords = Keywords(),
        Version = "1.0.0",
        Themes = [Vocab("ENER")],
        ConceptType = ConceptType.CodeList,
        ValidFrom = DateTimeOffset.UnixEpoch,
        ValidTo = DateTimeOffset.UnixEpoch.AddYears(1),
        ResponsiblePerson = Person(),
        ResponsibleDeputy = Person(),
    };

    public static MappingTableModel MappingTable() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["mt-1"],
        Publisher = Publisher(),
        System = System,
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Recorded,
        Name = Text("Name"),
        Description = Text("Beschreibung"),
        Keywords = Keywords(),
        Version = "1.0.0",
        Themes = [Vocab("ENER")],
        ValidFrom = DateTimeOffset.UnixEpoch,
        ValidTo = DateTimeOffset.UnixEpoch.AddYears(1),
        ResponsiblePerson = Person(),
        ResponsibleDeputy = Person(),
        Source = new MappingTableUriModel { Uri = "urn:source" },
        Target = new MappingTableUriModel { Uri = "urn:target" },
    };
}
