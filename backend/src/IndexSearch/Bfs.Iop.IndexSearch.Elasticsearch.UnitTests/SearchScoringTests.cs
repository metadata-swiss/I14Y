using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;
using Bfs.Iop.IndexSearch.Elasticsearch.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class SearchScoringTests
{
    private static readonly string[] German = ["de"];

    [Test]
    public void A_configured_field_weight_reaches_the_catalog_query()
    {
        var scoring = DefaultScoring.Catalog;
        scoring.FieldWeights[EsCatalogFields.Description] = 2.5;

        CatalogQuery("statistik", scoring).Should().Contain("description.de^2.5");
    }

    [Test]
    public void A_configured_partial_match_reaches_the_catalog_query()
    {
        var scoring = DefaultScoring.Catalog;
        scoring.PartialMatchBoost = 0.4;
        scoring.PartialMatchMinimumShouldMatch = "50%";

        var query = CatalogQuery("statistik", scoring);

        query.Should().Contain("\"boost\":0.4").And.Contain("\"minimum_should_match\":\"50%\"");
    }

    [Test]
    public void A_configured_recorded_weight_is_where_unweighted_documents_sort()
    {
        var scoring = DefaultScoring.Catalog;
        scoring.RegistrationStatusWeights[nameof(RegistrationStatus.Recorded)] = 42;

        using var body = JsonDocument.Parse(CatalogQuery(null, scoring));

        body.RootElement.GetProperty("sort").EnumerateArray()
            .Single(x => x.TryGetProperty(EsCatalogFields.RegistrationStatusWeight, out _))
            .GetProperty(EsCatalogFields.RegistrationStatusWeight)
            .GetProperty("missing").GetInt32().Should().Be(42);
    }

    [Test]
    public void A_configured_status_weight_is_written_into_the_document()
    {
        var scoring = DefaultScoring.Catalog;
        scoring.RegistrationStatusWeights[nameof(RegistrationStatus.Standard)] = 500;

        var document = CatalogDocumentFactory.Build(
            new CatalogIndexDocument
            {
                Id = Guid.NewGuid(),
                Type = SearchResourceType.Dataset,
                RegistrationStatus = RegistrationStatus.Standard,
            },
            scoring).Body;

        document[EsCatalogFields.RegistrationStatusWeight].Should().Be(500);
    }

    [Test]
    public void A_configured_boost_reaches_the_code_list_query()
    {
        var scoring = DefaultScoring.CodeList;
        scoring.CodeBoost = 33;
        scoring.AnnotationBoost = 3;
        scoring.NgramMinimumShouldMatch = "40%";

        var query = CodeListQuery("statistik", scoring);

        query.Should().Contain("\"boost\":33").And.Contain("\"boost\":3,").And.Contain("\"40%\"");
    }

    [Test]
    public void A_configured_typo_length_decides_how_many_typos_a_code_list_term_tolerates()
    {
        var scoring = DefaultScoring.CodeList;

        CodeListQuery("kanton", scoring).Should().Contain("\"fuzziness\":2");

        scoring.SingleEditMaxTermLength = 6;

        CodeListQuery("kanton", scoring).Should().Contain("\"fuzziness\":1").And.NotContain("\"fuzziness\":2");
    }

    [Test]
    public void Configured_values_override_single_defaults_and_keep_the_rest()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Elasticsearch:Uri"] = "http://elasticsearch.test:9200",
                ["Elasticsearch:CatalogIndexName"] = "i14y-catalog",
                ["Elasticsearch:CodeListIndexName"] = "i14y-codelist",
                ["SearchScoring:Catalog:FieldWeights:description"] = "0.5",
                ["SearchScoring:Catalog:RegistrationStatusWeights:Standard"] = "120",
                ["SearchScoring:CodeList:CodeBoost"] = "25",
            })
            .Build();

        var options = new ServiceCollection()
            .AddLogging()
            .AddIndexSearchElasticsearch(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<SearchScoringOptions>>()
            .Value;

        options.Catalog.FieldWeights[EsCatalogFields.Description].Should().Be(0.5);
        options.Catalog.FieldWeights[EsCatalogFields.Title].Should().Be(5);
        options.Catalog.RegistrationStatusWeights[nameof(RegistrationStatus.Standard)].Should().Be(120);
        options.Catalog.RegistrationStatusWeights[nameof(RegistrationStatus.Recorded)].Should().Be(100);
        options.CodeList.CodeBoost.Should().Be(25);
        options.CodeList.NameBoost.Should().Be(16);
    }

    private static string CatalogQuery(string? queryString, CatalogScoring scoring) =>
        JsonSerializer.Serialize(CatalogQueryBuilder.BuildSearchBody(
            queryString, German, null, SearchCaller.Anonymous, scoring, from: 0, size: 10));

    private static string CodeListQuery(string queryString, CodeListScoring scoring) =>
        JsonSerializer.Serialize(CodeListQueryBuilder.BuildSearchBody(
            Guid.NewGuid(), queryString, "de", filter: null, scoring, from: 0, size: 10));
}
