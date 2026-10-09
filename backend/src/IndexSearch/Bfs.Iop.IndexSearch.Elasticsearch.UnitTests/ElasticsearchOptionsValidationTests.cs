using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// Every one of these settings used to be read for the first time on the first request, so a bad value
// surfaced as a 500 per call while the host reported healthy. Checked at startup, the same value fails
// the rollout instead — but only if the check actually rejects it.
[TestFixture]
internal sealed class ElasticsearchOptionsValidationTests
{
    [Test]
    public void A_configured_node_and_two_index_names_are_accepted()
    {
        Validate(Options()).Failed.Should().BeFalse();
    }

    [TestCase("", TestName = "an empty uri")]
    [TestCase("   ", TestName = "a blank uri")]
    [TestCase("elasticsearch.test", TestName = "a uri with no scheme")]
    [TestCase("ftp://elasticsearch.test", TestName = "a uri that is not http")]
    public void A_node_that_cannot_be_called_is_rejected(string uri)
    {
        var result = Validate(Options(uri: uri));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Elasticsearch:Uri");
    }

    [Test]
    public void An_unsubstituted_placeholder_is_rejected()
    {
        // appsettings.json ships this token for the deployment pipeline to replace, and nothing in the
        // repository replaces it. It is not empty, so only a check for the token itself catches it.
        var result = Validate(Options(uri: "#{ELASTICSEARCH_URI}#"));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("placeholder");
    }

    [Test]
    public void A_missing_index_name_is_rejected_rather_than_defaulted()
    {
        var result = Validate(Options(catalog: ""));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("Elasticsearch:CatalogIndexName");
    }

    [Test]
    public void Two_identical_index_names_are_rejected()
    {
        // They resolve to one alias, so each rebuild pass would overwrite the other's documents and
        // every search would answer from the wrong corpus.
        var result = Validate(Options(catalog: "i14y-shared", codeList: "i14y-shared"));

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("different indices");
    }

    private static Microsoft.Extensions.Options.ValidateOptionsResult Validate(ElasticsearchOptions options) =>
        new ElasticsearchOptionsValidation().Validate(name: null, options);

    private static ElasticsearchOptions Options(
        string uri = "http://elasticsearch.test:9200",
        string catalog = "i14y-catalog",
        string codeList = "i14y-codelist") => new()
    {
        Uri = uri,
        CatalogIndexName = catalog,
        CodeListIndexName = codeList,
    };
}
