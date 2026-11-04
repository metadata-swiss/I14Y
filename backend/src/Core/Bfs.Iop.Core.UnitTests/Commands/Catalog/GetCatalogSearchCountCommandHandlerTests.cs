using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.CommandHandlers.Catalog;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Helpers;
using Bfs.Iop.DataAccess.Vocabularies;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using NSubstitute;

using CatalogFacetCounts = Bfs.Iop.IndexSearch.Contracts.Search.CatalogFacetCounts;
using CatalogFacetRequest = Bfs.Iop.IndexSearch.Contracts.Search.CatalogFacetRequest;

namespace Bfs.Iop.Core.UnitTests.Commands.Catalog;

[TestFixture(TestOf = typeof(GetCatalogSearchCountCommandHandler))]
internal sealed class GetCatalogSearchCountCommandHandlerTests
{
    private const string SwissTaxonomy = "Concept_DATASET_THEME";
    private const string EuTaxonomy = "VOCAB_EU_DATA_THEME";
    private const string SwissUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101";
    private const string EuUri = "http://publications.europa.eu/resource/authority/data-theme/101";

    private IIndexSearchApiClient _search = null!;
    private IAgentsService _agentsService = null!;
    private IVocabulariesService _vocabulariesService = null!;

    // Both taxonomies deliberately use the code "101" for two different themes.
    [SetUp]
    public void SetUp()
    {
        _search = Substitute.For<IIndexSearchApiClient>();
        _agentsService = Substitute.For<IAgentsService>();
        _vocabulariesService = Substitute.For<IVocabulariesService>();

        _vocabulariesService.TryGetVocabulary<ThemeTaxonomiesVocabulary>(Arg.Any<CancellationToken>())
            .Returns(new ThemeTaxonomiesVocabulary
            {
                Entries =
                [
                    new VocabularyEntryModel { VocabularyIdentifier = "VOCAB_I14Y_THEME_TAXONOMY", Code = SwissTaxonomy },
                    new VocabularyEntryModel { VocabularyIdentifier = "VOCAB_I14Y_THEME_TAXONOMY", Code = EuTaxonomy }
                ]
            });

        _vocabulariesService.TryGetVocabulary(SwissTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = SwissTaxonomy,
                Entries = [new VocabularyEntryModel { VocabularyIdentifier = SwissTaxonomy, Code = "101", Name = new MultiLanguageModel { En = "Labour" }, Uri = SwissUri }]
            });

        _vocabulariesService.TryGetVocabulary(EuTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = EuTaxonomy,
                Entries = [new VocabularyEntryModel { VocabularyIdentifier = EuTaxonomy, Code = "101", Name = new MultiLanguageModel { En = "Agriculture" }, Uri = EuUri }]
            });
    }

    private static string KeyOf(string vocabularyIdentifier, string code) =>
        ThemeSearchKeyHelper.BuildKey(new VocabularyEntryModel { Code = code, VocabularyIdentifier = vocabularyIdentifier });

    private void GivenThemeCounts(IReadOnlyDictionary<string, int> countByKey) =>
        _search.PostSearchCatalogFacetsByBodyAsync(Arg.Any<CatalogFacetRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SwaggerResponse<CatalogFacetCounts>(200, new Dictionary<string, IEnumerable<string>>(), new CatalogFacetCounts { Themes = countByKey }));

    private async Task<SearchCountResultModelThemes> WhenCounting()
    {
        var handler = new GetCatalogSearchCountCommandHandler(_search, _agentsService, _vocabulariesService);

        var result = await handler.Handle(new GetCatalogSearchCountCommand(null, null, new CatalogSearchFilter()), CancellationToken.None);

        return new SearchCountResultModelThemes(result.Themes.Select(x => (x.Value.Uri, x.Value.Name?.En, x.Count, x.SearchKey)).ToList());
    }

    private sealed record SearchCountResultModelThemes(IReadOnlyList<(string? Uri, string? Name, int Count, string? SearchKey)> Items);

    [Test]
    public async Task Given_two_taxonomies_sharing_a_code_When_counting_Then_both_themes_are_listed()
    {
        // Arrange
        GivenThemeCounts(new Dictionary<string, int>
        {
            [KeyOf(SwissTaxonomy, "101")] = 3,
            [KeyOf(EuTaxonomy, "101")] = 5
        });

        // Act
        var themes = await WhenCounting();

        // Assert
        themes.Items.Should().HaveCount(2);
        themes.Items.Should().ContainSingle(x => x.Uri == SwissUri && x.Name == "Labour" && x.Count == 3 && x.SearchKey == KeyOf(SwissTaxonomy, "101"));
        themes.Items.Should().ContainSingle(x => x.Uri == EuUri && x.Name == "Agriculture" && x.Count == 5 && x.SearchKey == KeyOf(EuTaxonomy, "101"));
    }

    [Test]
    public async Task Given_a_count_for_an_unknown_theme_When_counting_Then_it_is_left_out()
    {
        // Arrange
        GivenThemeCounts(new Dictionary<string, int> { [KeyOf("UNKNOWN_VOCAB", "999")] = 7 });

        // Act
        var themes = await WhenCounting();

        // Assert
        themes.Items.Should().BeEmpty();
    }
}
