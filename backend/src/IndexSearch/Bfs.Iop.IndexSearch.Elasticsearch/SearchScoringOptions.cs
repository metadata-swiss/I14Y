using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

/// <summary>
///     Every number that decides how search results are ranked. The defaults are the tuned values;
///     appsettings.json repeats them so they can be read in one place, and an environment overrides
///     single keys in Azure App Configuration. Read at startup, so a change needs a restart, and a
///     registration status weight also needs a rebuild because it is written into each document.
/// </summary>
public sealed class SearchScoringOptions
{
    public const string SectionName = "SearchScoring";

    public CatalogScoring Catalog { get; set; } = new();

    public CodeListScoring CodeList { get; set; } = new();
}

public sealed class CatalogScoring
{
    /// <summary>
    ///     How much each field counts towards relevance. What a resource is called, and the
    ///     identifiers people paste to find one exact thing, decide the match; everything else only
    ///     separates resources whose names matched equally well. A field left out counts 1.
    /// </summary>
    public Dictionary<string, double> FieldWeights { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [EsCatalogFields.Title] = 5,
        [EsCatalogFields.Name] = 5,
        [EsCatalogFields.Identifier] = 5,
        [EsCatalogFields.Version] = 5,
        [EsCatalogFields.Keyword] = 1,
        [EsCatalogFields.Description] = 0.3,
        [EsCatalogFields.ContactPointFn] = 0.2,
        [EsCatalogFields.ContactPointHasAddress] = 0.2,
        [EsCatalogFields.ContactPointNote] = 0.2,
        [EsCatalogFields.ContactPointHasTelephone] = 0.2,
        [EsCatalogFields.DataOwner] = 0.2,
        [EsCatalogFields.ResponsiblePersonName] = 0.2,
        [EsCatalogFields.ResponsibleDeputyName] = 0.2,
    };

    public double PartialMatchBoost { get; set; } = 0.75;

    public string PartialMatchMinimumShouldMatch { get; set; } = "75%";

    /// <summary>
    ///     The second sort key after relevance. A resource indexed before the field existed sorts as
    ///     though it were <see cref="RegistrationStatus.Recorded" />.
    /// </summary>
    public Dictionary<string, int> RegistrationStatusWeights { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(RegistrationStatus.Incomplete)] = 95,
        [nameof(RegistrationStatus.Candidate)] = 98,
        [nameof(RegistrationStatus.Recorded)] = 100,
        [nameof(RegistrationStatus.Qualified)] = 102,
        [nameof(RegistrationStatus.Standard)] = 105,
        [nameof(RegistrationStatus.PreferredStandard)] = 110,
        [nameof(RegistrationStatus.Superseded)] = 90,
        [nameof(RegistrationStatus.Retired)] = 85,
    };

    internal int RecordedWeight => WeightOf(RegistrationStatus.Recorded);

    internal int WeightOf(RegistrationStatus status) =>
        RegistrationStatusWeights.TryGetValue(status.ToString(), out var weight)
            ? weight
            : RegistrationStatusWeights[nameof(RegistrationStatus.Recorded)];
}

public sealed class CodeListScoring
{
    public double CodeBoost { get; set; } = 20;

    public double NameBoost { get; set; } = 16;

    public double DescriptionBoost { get; set; } = 12;

    public double AnnotationBoost { get; set; } = 8;

    public string NgramMinimumShouldMatch { get; set; } = "60%";

    /// <summary>
    ///     Terms up to this length tolerate one typo, longer ones two.
    /// </summary>
    public int SingleEditMaxTermLength { get; set; } = 4;
}
