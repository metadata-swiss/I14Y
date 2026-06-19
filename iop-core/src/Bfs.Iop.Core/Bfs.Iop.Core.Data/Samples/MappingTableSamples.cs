using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Samples;

internal static class MappingTableSamples
{
    public static readonly Guid DatasetThemeToEuDataThemeId =
        new("703b7365-5090-4247-ad70-ae98cbc77df6");

    private const string SourceConceptIdentifier = "DV_DCAT_DATASET_THEME";
    private const string SourceConceptVersion = "1.1.0";

    private const string TargetConceptIdentifier = "VOCAB_EU_DATA_THEME";
    private const string TargetConceptVersion = "20220715.0.0";

    private const string RelationTypeSameAs = "sameAs";

    public static IEnumerable<MappingTable> Generate()
    {
        var mappingTable = new MappingTable
        {
            Id = DatasetThemeToEuDataThemeId,
            Identifiers = ["mapping-dv-dcat-dataset-theme-eu-data-theme"],
            Version = "1.0.0",
            PublisherId = AgentSamples.I14YTestId,
            ResponsiblePersonId = IopPersonSamples.MaxMusterId,
            ResponsibleDeputyId = null,
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            Name = new MultiLanguage
            {
                De = "Mapping I14Y Themenvokabular zu EU Data Theme",
                En = "Mapping I14Y dataset themes to EU data themes",
                Fr = "Mapping des thèmes I14Y vers les thèmes de données EU",
                It = "Mapping dei temi I14Y verso i temi dei dati UE"
            },
            Description = new MultiLanguage
            {
                En = "This mapping table associates I14Y dataset theme codes with the corresponding EU data theme codes used by DCAT-AP.",
                Fr = "Cette table de mapping associe les codes des thèmes I14Y aux codes de thèmes de données EU utilisés par DCAT-AP."
            },
            SourceUri = BuildConceptUri(SourceConceptIdentifier, SourceConceptVersion),
            TargetUri = BuildConceptUri(TargetConceptIdentifier, TargetConceptVersion),
            Themes = [],
            ConformsTo = [],
            Keywords = [],
            ValidFrom = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidTo = null,
            Relations =
            [
                CreateRelation("115", "ECON"),
                CreateRelation("109", "AGRI"),
                CreateRelation("116", "TRAN")
            ]
        };

        foreach (MappingRelation relation in mappingTable.Relations)
        {
            relation.MappingTable = mappingTable;
        }

        return [mappingTable];
    }

    private static MappingRelation CreateRelation(
        string sourceCode,
        string targetCode) =>
        new()
        {
            SourceCodeUri = BuildCodeUri(
                SourceConceptIdentifier,
                sourceCode,
                SourceConceptVersion),
            TargetCodeUri = BuildCodeUri(
                TargetConceptIdentifier,
                targetCode,
                TargetConceptVersion),
            RelationType = RelationTypeSameAs
        };

    private static string BuildConceptUri(
        string conceptIdentifier,
        string conceptVersion) =>
        $"https://register.ld.admin.ch/i14y/concept/{conceptIdentifier}/version/{conceptVersion}";

    private static string BuildCodeUri(
        string conceptIdentifier,
        string code,
        string conceptVersion) =>
        $"https://register.ld.admin.ch/i14y/concept/{conceptIdentifier}/{code}/version/{conceptVersion}";
}