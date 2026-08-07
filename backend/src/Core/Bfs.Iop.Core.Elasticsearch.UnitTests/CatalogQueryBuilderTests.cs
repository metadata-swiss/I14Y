using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Elasticsearch;

namespace Bfs.Iop.Core.Elasticsearch.UnitTests;

/// <summary>
/// Tests for the reuse-count / registration-status function_score wrapper built by
/// <see cref="CatalogQueryBuilder"/>. <c>BuildSearchBody</c> is pure (returns a
/// <see cref="Dictionary{TKey,TValue}"/>, no I/O), so the JSON shape can be asserted directly
/// without a running Elasticsearch instance.
/// </summary>
[TestFixture(TestOf = typeof(CatalogQueryBuilder))]
public class CatalogQueryBuilderTests
{
    private static readonly string[] Languages = ["de", "en"];
    private static readonly string[] NoAgencies = [];

    [Test]
    public void BuildSearchBody_WithoutQueryString_ReuseWeightFunctionAppliesOnlyToConcepts()
    {
        var body = CatalogQueryBuilder.BuildSearchBody(
            queryString: null, Languages, filter: null, BusinessRole.InteroperabilityService, NoAgencies, from: 0, size: 10);

        var functions = GetFunctions(body);

        Assert.That(functions, Has.Count.EqualTo(1), "Without a search term, only the reuse-weight function should be present.");

        var reuseFunction = (Dictionary<string, object?>)functions[0];
        var filter = (Dictionary<string, object?>)reuseFunction["filter"]!;
        var term = (Dictionary<string, object?>)filter["term"]!;
        Assert.That(term[EsCatalogFields.Type], Is.EqualTo(nameof(SearchResourceType.Concept)));

        var fieldValueFactor = (Dictionary<string, object?>)reuseFunction["field_value_factor"]!;
        Assert.That(fieldValueFactor["field"], Is.EqualTo(EsCatalogFields.ReuseWeight));
    }

    [Test]
    public void BuildSearchBody_WithQueryString_IncludesBothReuseWeightAndRegistrationStatusFunctions()
    {
        var body = CatalogQueryBuilder.BuildSearchBody(
            queryString: "test", Languages, filter: null, BusinessRole.InteroperabilityService, NoAgencies, from: 0, size: 10);

        var functions = GetFunctions(body);

        Assert.That(functions, Has.Count.EqualTo(2), "With a search term, both functions should apply.");

        var fields = functions
            .Select(f => (Dictionary<string, object?>)f)
            .Select(f => (Dictionary<string, object?>)f["field_value_factor"]!)
            .Select(fvf => fvf["field"])
            .ToArray();

        Assert.That(fields, Contains.Item(EsCatalogFields.ReuseWeight));
        Assert.That(fields, Contains.Item(EsCatalogFields.RegistrationStatusWeight));
    }

    [Test]
    public void BuildSearchBody_FunctionScore_UsesMultiplyModes()
    {
        var body = CatalogQueryBuilder.BuildSearchBody(
            queryString: "test", Languages, filter: null, BusinessRole.InteroperabilityService, NoAgencies, from: 0, size: 10);

        var functionScore = GetFunctionScore(body);

        Assert.That(functionScore["score_mode"], Is.EqualTo("multiply"));
        Assert.That(functionScore["boost_mode"], Is.EqualTo("multiply"));
    }

    private static Dictionary<string, object?> GetFunctionScore(Dictionary<string, object?> body)
    {
        var query = (Dictionary<string, object?>)body["query"]!;
        var boolQuery = (Dictionary<string, object?>)query["bool"]!;
        var must = (object[])boolQuery["must"]!;
        var scored = (Dictionary<string, object?>)must[0];
        return (Dictionary<string, object?>)scored["function_score"]!;
    }

    private static List<object> GetFunctions(Dictionary<string, object?> body) =>
        (List<object>)GetFunctionScore(body)["functions"]!;
}
