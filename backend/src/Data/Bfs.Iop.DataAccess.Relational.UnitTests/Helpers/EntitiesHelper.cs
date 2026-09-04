using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;

internal static class EntitiesHelper
{
    public static Agent Agent => new()
    {
        Id = new Guid("0f10e7df-0632-4ea7-8563-3c73cdb86f63"),
        Identifier = "organisation",
        Name = MultiLanguage,
        PrefLabel = MultiLanguage,
    };

    public static Annotation Annotation => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "Annotation_identifier",
        Position = 69,
        Text = MultiLanguage,
        Title = "Something_new",
        Type = "Something_newer",
        Uri = "foo://example.com:8042/over/there?name=ferret#nose"
    };

    public static CodeListEntry CodeListEntry => new()
    {
        Annotations = [Annotation],
        Code = "12345",
        Description = MultiLanguage,
        Id = Guid.NewGuid(),
        Name = MultiLanguage,
        Position = 96
    };

    public static Dataset Dataset => new()
    {
        Id = Guid.NewGuid(),
        Identifier = ["Dataset_identifier",],
        PublisherId = Agent.Id,
    };

    public static DataService DataService => new()
    {
        AccessRights = "something",
        Id = Guid.NewGuid(),
        PublisherId = Agent.Id,
        Theme = []
    };

    public static DcatCatalog DcatCatalog => new()
    {
        Id = Guid.NewGuid(),
        Description = MultiLanguage,
        PublisherId = Agent.Id,
        ThemeTaxonomy = ["Theme1", "Theme2"],
        Title = MultiLanguage
    };

    public static Distribution Distribution => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "Distribution_identifier"
    };

    public static IopConcept IopConcept => new()
    {
        ConceptType = ConceptType.Date,
        ConformsTo = [Resource],
        Description = MultiLanguage,
        Id = Guid.NewGuid(),
        Identifiers = ["Iop_concept_identifier"],
        Keywords = [Keyword],
        Name = MultiLanguage,
        Pattern = "dd/mm/yyyy",
        PublisherId = Agent.Id,
        ResponsibleDeputyId = IopPerson.Id,
        ResponsiblePersonId = IopPerson2.Id,
        Themes = ["100"],
        ValidFrom = new DateTimeOffset(2024, 12, 12, 0, 0, 0, TimeSpan.Zero),
        ValidTo = new DateTimeOffset(2030, 12, 12, 0, 0, 0, TimeSpan.Zero),
        Version = "1.0.0"
    };

    public static IopPerson IopPerson => new()
    {
        Email = "abc@mail.ch",
        FamilyName = "Doe",
        GivenName = "John",
        Id = new Guid("7294399b-a443-426d-96e1-0c02a1528742"),
        FirstLoginDate = new DateOnly(2024, 1, 1),
        LastLoginDate = DateOnly.FromDateTime(DateTime.Now)
    };

    public static IopPerson IopPerson2 => new()
    {
        Email = "def@mail.ch",
        FamilyName = "Fish",
        GivenName = "Max",
        Id = new Guid("36b2cdf9-6a31-44c9-b2da-444ca0fca1e9"),
        FirstLoginDate = new DateOnly(2024, 1, 1),
        LastLoginDate = DateOnly.FromDateTime(DateTime.Now)
    };

    public static Keyword Keyword => new()
    {
        Id = Guid.NewGuid(),
        Text = MultiLanguage,
    };

    public static MappingTable MappingTable => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["MappingTable_identifier"],
        Name = MultiLanguage,
        PublisherId = Agent.Id,
        ResponsibleDeputyId = IopPerson.Id,
        ResponsiblePersonId = IopPerson2.Id,
        SourceUri = "http://www.mywebsite.com",
        TargetUri = "http://www.mywebsite2.com",
        Version = "1.0.0"
    };

    public static MultiLanguage MultiLanguage => new()
    {
        De = "Deutsch",
        En = "English",
        Fr = "Français",
        It = "Italiano",
        Rm = "??"
    };

    public static PublicService PublicService => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["PublicService_Identifier"],
        Sector = [],
        ThematicArea = []
    };

    public static QualifiedAttribution QualifiedAttribution => new()
    {
        AgentId = Agent.Id,
        Id = Guid.NewGuid(),
        HadRole = "fake_role"
    };

    public static QualifiedRelation QualifiedRelation => new()
    {
        Id = Guid.NewGuid(),
        HadRole = "fake_role",
        Relation = Resource
    };

    public static Resource Resource => new()
    {
        Href = "telnet://192.0.2.16:80",
        Id = Guid.NewGuid(),
        Label = MultiLanguage,
    };

    public static VocabularyConfig VocabularyConfig => new()
    {
        Id = Guid.NewGuid(),
        VocabularyIdentifier = "TestVocabulary",
        ConceptIdentifier = "TestConcept",
        ConceptVersion = "1.0.0"
    };
}
