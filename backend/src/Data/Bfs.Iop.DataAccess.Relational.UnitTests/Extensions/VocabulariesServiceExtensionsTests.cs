using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Extensions;

[TestFixture(TestOf = typeof(VocabulariesServiceExtensions))]
internal sealed class VocabulariesServiceExtensionsTests
{
    private const string SwissTaxonomy = "Concept_DATASET_THEME";
    private const string EuTaxonomy = "VOCAB_EU_DATA_THEME";
    private const string SwissUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101";
    private const string EuUri = "http://publications.europa.eu/resource/authority/data-theme/AGRI";

    private IVocabulariesService _vocabulariesService = null!;

    [SetUp]
    public void SetUp() => _vocabulariesService = Substitute.For<IVocabulariesService>();

    private void GivenRegistry(params string[] taxonomyIdentifiers) =>
        _vocabulariesService.TryGetVocabulary<ThemeTaxonomiesVocabulary>(Arg.Any<CancellationToken>())
            .Returns(new ThemeTaxonomiesVocabulary
            {
                Entries = [.. taxonomyIdentifiers.Select(x => new VocabularyEntryModel { Code = x })]
            });

    private void GivenVocabulary(string identifier, params VocabularyEntryModel[] entries) =>
        _vocabulariesService.TryGetVocabulary(identifier, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel { Identifier = identifier, Entries = entries });

    // Two taxonomies, each contributing its own theme: the shape the feature actually runs in.
    private void GivenSwissAndEuTaxonomies()
    {
        GivenRegistry(SwissTaxonomy, EuTaxonomy);

        GivenVocabulary(SwissTaxonomy,
            new VocabularyEntryModel { Code = "101", Name = new MultiLanguageModel { En = "Labour" }, Uri = SwissUri });

        GivenVocabulary(EuTaxonomy,
            new VocabularyEntryModel { Code = "AGRI", Name = new MultiLanguageModel { En = "Agriculture" }, Uri = EuUri });
    }

    [Test]
    public void Given_two_registered_taxonomies_When_getting_themes_Then_entries_of_both_are_returned()
    {
        // Arrange
        GivenSwissAndEuTaxonomies();

        // Act
        var result = _vocabulariesService.GetThemes();

        // Assert
        result.Select(x => x.Uri).Should().BeEquivalentTo([SwissUri, EuUri]);
    }

    [Test]
    public void Given_an_entry_without_uri_When_getting_themes_Then_it_is_skipped()
    {
        // Arrange
        GivenRegistry(SwissTaxonomy);

        GivenVocabulary(SwissTaxonomy,
            new VocabularyEntryModel { Code = "101", Uri = SwissUri },
            new VocabularyEntryModel { Code = "999", Uri = null });

        // Act
        var result = _vocabulariesService.GetThemes();

        // Assert
        result.Should().ContainSingle().Which.Code.Should().Be("101");
    }

    [Test]
    public void Given_a_taxonomy_that_resolves_to_no_vocabulary_When_getting_themes_Then_it_is_skipped()
    {
        // Arrange
        GivenRegistry("UNKNOWN");

        _vocabulariesService.TryGetVocabulary("UNKNOWN", Arg.Any<CancellationToken>())
            .Returns((VocabularyModel?)null);

        // Act
        var result = _vocabulariesService.GetThemes();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void Given_an_input_with_a_uri_When_resolving_to_uri_Then_it_is_returned_verbatim()
    {
        // Act
        var result = _vocabulariesService.ResolveThemeInputToUri(new ThemeInputModel { Uri = SwissUri });

        // Assert
        result.Should().Be(SwissUri);
    }

    [Test]
    public void Given_an_input_with_a_code_When_resolving_to_uri_Then_the_uri_of_its_taxonomy_is_returned()
    {
        // Arrange
        GivenSwissAndEuTaxonomies();

        // Act & Assert
        _vocabulariesService.ResolveThemeInputToUri(new ThemeInputModel { Code = "101" }).Should().Be(SwissUri);
        _vocabulariesService.ResolveThemeInputToUri(new ThemeInputModel { Code = "AGRI" }).Should().Be(EuUri);
    }

    [Test]
    public void Given_an_input_with_an_unknown_code_When_resolving_to_uri_Then_throw_error()
    {
        // Arrange
        GivenSwissAndEuTaxonomies();

        // Act
        Action act = () => _vocabulariesService.ResolveThemeInputToUri(new ThemeInputModel { Code = "unknown" });

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Given_stored_uris_from_two_taxonomies_When_resolving_values_Then_code_and_name_are_returned()
    {
        // Arrange
        GivenSwissAndEuTaxonomies();

        // Act
        IReadOnlyList<VocabularyEntryModel> result = _vocabulariesService.ResolveThemeValues([EuUri, SwissUri]);

        // Assert
        result.Select(x => x.Code).Should().Equal("AGRI", "101");
        result.Select(x => x.Name!.En).Should().Equal("Agriculture", "Labour");
    }

    [Test]
    public void Given_a_stored_value_that_resolves_to_nothing_When_resolving_values_Then_it_is_returned_as_is()
    {
        // Arrange
        GivenSwissAndEuTaxonomies();

        var staleUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/999";

        // Act
        IReadOnlyList<VocabularyEntryModel> result = _vocabulariesService.ResolveThemeValues([staleUri]);

        // Assert
        result.Should().ContainSingle();
        result[0].Uri.Should().Be(staleUri);
        result[0].Code.Should().Be(staleUri);
    }
}
