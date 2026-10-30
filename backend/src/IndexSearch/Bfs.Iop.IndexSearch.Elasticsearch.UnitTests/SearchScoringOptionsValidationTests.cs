using AwesomeAssertions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class SearchScoringOptionsValidationTests
{
    [Test]
    public void The_shipped_default_is_accepted()
    {
        Validate(new SearchScoringOptions()).Failed.Should().BeFalse();
    }

    [Test]
    public void A_field_the_search_does_not_search_is_rejected()
    {
        var options = new SearchScoringOptions();
        options.Catalog.FieldWeights["titel"] = 5;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SearchScoring:Catalog:FieldWeights").And.Contain("titel");
    }

    [Test]
    public void A_status_that_does_not_exist_is_rejected()
    {
        var options = new SearchScoringOptions();
        options.Catalog.RegistrationStatusWeights["Standrd"] = 105;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SearchScoring:Catalog:RegistrationStatusWeights").And.Contain("Standrd");
    }

    [TestCase(0, TestName = "a weight of zero")]
    [TestCase(-1, TestName = "a negative weight")]
    [TestCase(double.NaN, TestName = "a weight that is not a number")]
    public void A_field_weight_that_is_not_positive_is_rejected(double weight)
    {
        var options = new SearchScoringOptions();
        options.Catalog.FieldWeights[EsCatalogFields.Description] = weight;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SearchScoring:Catalog:FieldWeights:description");
    }

    [Test]
    public void A_boost_that_is_not_positive_is_rejected()
    {
        var options = new SearchScoringOptions();
        options.Catalog.PartialMatchBoost = 0;
        options.CodeList.CodeBoost = -2;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should()
            .Contain("SearchScoring:Catalog:PartialMatchBoost")
            .And.Contain("SearchScoring:CodeList:CodeBoost");
    }

    [TestCase("75")]
    [TestCase("60%")]
    [TestCase("-25%")]
    public void A_minimum_should_match_Elasticsearch_understands_is_accepted(string value)
    {
        var options = new SearchScoringOptions();
        options.Catalog.PartialMatchMinimumShouldMatch = value;
        options.CodeList.NgramMinimumShouldMatch = value;

        Validate(options).Failed.Should().BeFalse();
    }

    [TestCase("", TestName = "an empty minimum should match")]
    [TestCase("75 %", TestName = "a minimum should match with a space")]
    [TestCase("0.75", TestName = "a minimum should match as a fraction")]
    public void A_minimum_should_match_Elasticsearch_rejects_is_rejected(string value)
    {
        var options = new SearchScoringOptions();
        options.CodeList.NgramMinimumShouldMatch = value;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SearchScoring:CodeList:NgramMinimumShouldMatch");
    }

    [Test]
    public void A_negative_typo_length_is_rejected()
    {
        var options = new SearchScoringOptions();
        options.CodeList.SingleEditMaxTermLength = -1;

        var result = Validate(options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SearchScoring:CodeList:SingleEditMaxTermLength");
    }

    private static ValidateOptionsResult Validate(SearchScoringOptions options) =>
        new SearchScoringOptionsValidation().Validate(name: null, options);
}
