using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Vocabularies;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Extensions;

[TestFixture(TestOf = typeof(CatalogSearchFilterExtensions))]
internal sealed class CatalogSearchFilterExtensionsTests
{
    private const string SwissTaxonomy = "Concept_DATASET_THEME";
    private const string EuTaxonomy = "VOCAB_EU_DATA_THEME";

    private IVocabulariesService _vocabulariesService = null!;

    [SetUp]
    public void SetUp()
    {
        _vocabulariesService = Substitute.For<IVocabulariesService>();

        _vocabulariesService.TryGetVocabulary<ThemeTaxonomiesVocabulary>(Arg.Any<CancellationToken>())
            .Returns(new ThemeTaxonomiesVocabulary
            {
                Entries =
                [
                    new VocabularyEntryModel { Code = SwissTaxonomy },
                    new VocabularyEntryModel { Code = EuTaxonomy }
                ]
            });

        _vocabulariesService.TryGetVocabulary(SwissTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = SwissTaxonomy,
                Entries = [new VocabularyEntryModel { Code = "101", Uri = "https://register.ld.admin.ch/101" }]
            });

        _vocabulariesService.TryGetVocabulary(EuTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = EuTaxonomy,
                Entries =
                [
                    new VocabularyEntryModel { Code = "101", Uri = "http://publications.europa.eu/101" },
                    new VocabularyEntryModel { Code = "AGRI", Uri = "http://publications.europa.eu/AGRI" }
                ]
            });
    }

    private static string KeyOf(string vocabularyIdentifier, string code) =>
        ThemeSearchKey.For(new VocabularyEntryModel { Code = code, VocabularyIdentifier = vocabularyIdentifier });

    private IEnumerable<string> WhenResolving(params string[] themes) =>
        new CatalogSearchFilter { Themes = themes }.WithResolvedThemes(_vocabulariesService).Themes;

    [Test]
    public void Given_a_theme_key_When_resolving_Then_it_passes_through()
    {
        // Act & Assert
        WhenResolving(KeyOf(EuTaxonomy, "AGRI")).Should().Equal(KeyOf(EuTaxonomy, "AGRI"));
    }

    [Test]
    public void Given_a_bare_code_shared_by_two_taxonomies_When_resolving_Then_both_themes_are_filtered()
    {
        // Arrange

        // Act & Assert
        WhenResolving("101").Should().BeEquivalentTo([KeyOf(SwissTaxonomy, "101"), KeyOf(EuTaxonomy, "101")]);
    }

    [Test]
    public void Given_a_bare_code_of_a_single_taxonomy_When_resolving_Then_that_theme_is_filtered()
    {
        // Act & Assert
        WhenResolving("AGRI").Should().Equal(KeyOf(EuTaxonomy, "AGRI"));
    }

    [Test]
    public void Given_a_value_naming_no_theme_When_resolving_Then_it_is_kept_as_is()
    {
        // Arrange

        // Act & Assert
        WhenResolving("unknown").Should().Equal("unknown");
    }

    [Test]
    public void Given_no_theme_When_resolving_Then_the_filter_is_untouched()
    {
        // Arrange
        var filter = new CatalogSearchFilter();

        // Act & Assert
        filter.WithResolvedThemes(_vocabulariesService).Should().BeSameAs(filter);
    }
}
