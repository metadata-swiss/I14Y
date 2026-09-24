using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Samples;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Samples;

/// <summary>
/// Checks the seeded vocabulary files against what the theme-by-URI feature needs of them. A theme
/// without a URI is silently dropped and a duplicated code silently resolves to the wrong theme, so
/// neither shows up as a failure at runtime — only here.
/// </summary>
[TestFixture(TestOf = typeof(IopConceptSamples))]
internal sealed class ThemeTaxonomySeedTests
{
    private const string ExtResource = "EXT_RESOURCE";

    private static readonly string RegistryIdentifier = new ThemeTaxonomiesVocabulary().Identifier;

    private static IopConcept? FindConceptForVocabulary(string vocabularyIdentifier)
    {
        var config = VocabularyConfigSamples.Generate()
            .SingleOrDefault(x => x.VocabularyIdentifier == vocabularyIdentifier);

        return config is null
            ? null
            : IopConceptSamples.Generate()
                .SingleOrDefault(x => x.Identifiers.Contains(config.ConceptIdentifier) && x.Version == config.ConceptVersion);
    }

    // The registry entries: one per vocabulary usable as a theme source, keyed by vocabulary identifier.
    private static IReadOnlyList<CodeListEntry> GetThemeTaxonomies() =>
        [.. FindConceptForVocabulary(RegistryIdentifier)?.CodeListEntries ?? []];

    private static string? TryGetUri(CodeListEntry entry) =>
        entry.Annotations.FirstOrDefault(x => x.Type == ExtResource)?.Uri;

    private static IEnumerable<(string Taxonomy, string Code, string? Uri)> GetAllThemes() =>
        GetThemeTaxonomies().SelectMany(taxonomy =>
            (FindConceptForVocabulary(taxonomy.Code)?.CodeListEntries ?? [])
                .Select(entry => (Taxonomy: taxonomy.Code, entry.Code, Uri: TryGetUri(entry))));

    [Test]
    public void Given_the_seeded_registry_When_reading_its_taxonomies_Then_each_resolves_to_a_seeded_vocabulary()
    {
        // Arrange
        var taxonomies = GetThemeTaxonomies();

        // Assert
        taxonomies.Should().NotBeEmpty($"'{RegistryIdentifier}' must list at least one theme taxonomy");

        using var _ = new AssertionScope();

        foreach (var taxonomy in taxonomies)
        {
            FindConceptForVocabulary(taxonomy.Code).Should().NotBeNull(
                $"'{taxonomy.Code}' is listed as a theme taxonomy but is not a seeded vocabulary");
        }
    }

    [Test]
    public void Given_the_seeded_taxonomies_When_reading_their_themes_Then_each_carries_an_ext_resource_uri()
    {
        // Arrange
        var themes = GetAllThemes().ToList();

        // Assert
        themes.Should().NotBeEmpty();

        using var _ = new AssertionScope();

        foreach (var theme in themes)
        {
            theme.Uri.Should().NotBeNullOrWhiteSpace(
                $"'{theme.Code}' of '{theme.Taxonomy}' is unusable as a theme without a URI");
        }
    }

    [Test]
    public void Given_the_seeded_taxonomies_When_reading_their_themes_Then_codes_and_uris_are_unique()
    {
        // Arrange
        var themes = GetAllThemes().ToList();

        // Assert
        using var _ = new AssertionScope();

        // The Lucene theme facet is keyed by code, and resolving an input code picks the first match,
        // so two taxonomies sharing a code would quietly conflate two different themes.
        themes.Should().OnlyHaveUniqueItems(x => x.Code);
        themes.Should().OnlyHaveUniqueItems(x => x.Uri);
    }
}
