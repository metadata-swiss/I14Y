using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.UnitTests.Helpers;

internal static class ModelsHelper
{
    public static AgentModel AgentModel => new()
    {
        Identifier = "agentIdentifier",
        Name = MultiLanguageModel,
        PrefLabel = MultiLanguageModel,
        System = SystemInfoModel
    };

    public static AllowActionResult AllowActionResultRead => new()
    {
        ActionType = AllowActionType.Read,
        Value = true
    };

    public static AnnotationInputModel AnnotationInputModel => new()
    {
        Type = "annotation_type",
        Identifier = "annotation_identifier"
    };

    public static ChannelInputModel ChannelInputModel => new()
    {
        Identifier = "channel_identifier"
    };

    public static CodeListEntryInputModel CodeListEntryInputModel => new()
    {
        Code = "code",
        Name = MultiLanguageModel,
    };

    public static CodeListEntryInputModel CodeListEntryInputModelWithAnnotation => new()
    {
        Code = "code",
        Name = MultiLanguageModel,
        Annotations = [AnnotationInputModel]
    };

    public static DataServiceInputModel DataServiceInputModel => new()
    {
        AccessRights = new CodeInputModel() { Code = "PUBLIC" },
        ContactPoints = [VCardModel],
        Description = MultiLanguageModel,
        EndpointUrls = [ResourceModel],
        Publisher = new IdentifierInputModel() { Identifier = "toto " },
        Title = MultiLanguageModel
    };

    public static DcatCatalogModel DcatCatalogModel => new()
    {
        Title = MultiLanguageModel,
        Description = MultiLanguageModel,
        Publisher = AgentModel,
        System = SystemInfoModel
    };

    public static DcatCatalogResourceModel DcatCatalogResourceModelDataservice => new()
    {
        ResourceType = DcatCatalogType.DataService
    };

    public static DcatCatalogResourceModel DcatCatalogResourceModelDataset => new()
    {
        ResourceType = DcatCatalogType.Dataset
    };

    public static DcatCatalogRecordModel DcatCatalogRecordModelDataservice => new()
    {
        PrimaryTopic = DcatCatalogResourceModelDataservice
    };

    public static DcatCatalogRecordModel DcatCatalogRecordModelDataset => new()
    {
        PrimaryTopic = DcatCatalogResourceModelDataset
    };

    public static DataServiceModel DataServiceModel => new()
    {
        Id = new Guid("3e6b3536-59a1-44bb-a2aa-f0aaa1ce82d7"),
        Identifiers = ["3e6b3536-59a1-44bb-a2aa-f0aaa1ce82d7"],
        AccessRights = VocabularyEntryModelAccessRightPublic,
        ContactPoints = [VCardModel],
        Documentation = [],
        EndpointDescriptions = [],
        EndpointUrls = [],
        Keywords = [],
        LandingPages = [],
        Publisher = AgentModel,
        System = SystemInfoModel,
        Title = MultiLanguageModel,
        ServesDatasets = []
    };

    public static DcatDatasetModel DcatDatasetModel => new()
    {
        Identifiers = ["dataset_identifier"],
        Distributions = [DcatDistributionModel],
        Description = MultiLanguageModel,
        Publisher = AgentModel,
        Title = MultiLanguageModel,
        AccessRights = VocabularyEntryModelAccessRightPublic,
        ContactPoints = [],
        Documentation = [],
        ConformsTo = [],
        Images = [],
        IsReferencedBy = [],
        Relations = [],
        Keywords = [],
        LandingPages = [],
        Languages = [],
        QualifiedAttributions = [],
        QualifiedRelations = [],
        Spatial = [],
        System = SystemInfoModel,
        TemporalCoverage = [],
        Issued = DateTime.UtcNow,
        Modified = DateTime.UtcNow
    };

    public static DcatDatasetInputModel DcatDatasetInputModel => new()
    {
        AccessRights = new CodeInputModel() { Code = "PUBLIC" },
        ContactPoints = [VCardModel],
        Description = MultiLanguageModel,
        Identifiers = ["identifier1", "identifier2"],
        Publisher = new IdentifierInputModel() { Identifier = "toto" },
        Title = MultiLanguageModel,
    };

    public static DcatDistributionModel DcatDistributionModel => new()
    {
        AccessUrl = ResourceModel,
        Description = MultiLanguageModel,
        Title = MultiLanguageModel,
        AccessServices = [new IdModel() { Id = new Guid(DataServiceModel.Identifiers.First())}]
    };

    public static DcatDistributionInputModel DcatDistributionInputModel => new()
    {
        AccessUrl = ResourceModel,
        Description = MultiLanguageModel,
        Title = MultiLanguageModel,
    };

    public static IopConceptInputModel IopConceptInputModel => new()
    {
        ConceptType = ConceptType.String,
        Description = MultiLanguageModel,
        Identifiers = ["concept_identifier"],
        MinLength = 1,
        MaxLength = 10,
        Name = MultiLanguageModel,
        Publisher = new IdentifierInputModel() { Identifier = "organisation" },
        ResponsibleDeputy = new EmailInputModel() { Email = "abc@mail.ch" },
        ResponsiblePerson = new EmailInputModel() { Email = "def@mail.ch" },
        ValidFrom = new DateTimeOffset(2020, 10, 13, 0, 0, 0, TimeSpan.Zero),
        Version = "1.0.0"
    };

    public static IopConceptInputModel IopConceptInputModelCodeList => new()
    {
        ConceptType = ConceptType.CodeList,
        Description = MultiLanguageModel,
        Identifiers = ["concept_identifier"],
        Name = MultiLanguageModel,
        Publisher = new IdentifierInputModel() { Identifier = "organisation" },
        ResponsibleDeputy = new EmailInputModel() { Email = "abc@mail.ch" },
        ResponsiblePerson = new EmailInputModel() { Email = "def@mail.ch" },
        ValidFrom = new DateTimeOffset(2020, 10, 13, 0, 0, 0, TimeSpan.Zero),
        Version = "1.0.0",
        CodeListEntryValueMaxLength = 255,
        CodeListEntryValueType = CodeListEntryValueType.String,
    };

    public static MappingTableInputModel MappingTableInputModel => new()
    {
        Description = MultiLanguageModel,
        Name = MultiLanguageModel,
        Publisher = new IdentifierInputModel() { Identifier = "organisation" },
        ResponsibleDeputy = new EmailInputModel() { Email = "abc@mail.ch" },
        ResponsiblePerson = new EmailInputModel() { Email = "def@mail.ch" },
        Source = new UriInputModel() { Uri = "http://www.mywebsite.com" },
        Target = new UriInputModel() { Uri = "http://www.mywebsite2.com" },
        ValidFrom = new DateTimeOffset(),
        Version = "1.0.0"
    };

    public static MultiLanguageModel MultiLanguageModel => new()
    {
        De = "Deutsch",
        En = "English",
        Fr = "Français",
        It = "Italiano",
        Rm = "??"
    };

    public static ResourceModel ResourceModel => new()
    {
        Uri = "https://www.example.com/index.html",
    };

    public static SystemInfoModel SystemInfoModel => new()
    {
        CreatedAt = new DateTimeOffset(2026, 3, 23, 0, 0, 0, TimeSpan.Zero),
        CreationType = CreationType.Manual
    };

    public static VCardModel VCardModel => new()
    {
        HasEmail = "toto@tata.ch"
    };

    public static VocabularyConfigInputModel VocabularyConfigInputModel => new()
    {
        VocabularyIdentifier = "NewVocabulary",
        ConceptIdentifier = "NewConcept",
        ConceptVersion = "2.0.0"
    };

    public static VocabularyEntryModel VocabularyEntryModelAccessRightPublic => new()
    {
        Code = "PUBLIC",
        Uri = "http://publications.europa.eu/resource/authority/access-right/PUBLIC"
    };

    public static Dictionary<ConceptType, IopConceptInputModel> IopConceptInputModels => new()
    {
        { ConceptType.CodeList, IopConceptInputModelCodeList },
        { ConceptType.String, IopConceptInputModel }
    };
}