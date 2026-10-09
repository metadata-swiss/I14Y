using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Contracts;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

[TestFixture]
internal sealed class SearchLanguageTests
{
    [Test]
    public void A_catalogue_search_runs_in_every_language_whatever_the_caller_asks_for()
    {
        // Narrowing to one language hid resources whose title happens to be recorded only in another,
        // and falling back to German when the caller named none made German the silent default for
        // everybody. A catalogue search looks everywhere; the language parameter is not consulted.
        SearchController.Languages(null).Should().Equal(IndexLanguages.All);
        SearchController.Languages("fr").Should().Equal(IndexLanguages.All);
        SearchController.Languages("nonsense").Should().Equal(IndexLanguages.All);
    }
    [Test]
    public void A_caller_that_names_no_language_gets_german()
    {
        SearchController.Language(null).Should().Be(IndexLanguages.Default);
        SearchController.Language(string.Empty).Should().Be(IndexLanguages.Default);
        SearchController.Language("   ").Should().Be(IndexLanguages.Default);
    }

    [TestCase("de")]
    [TestCase("en")]
    [TestCase("fr")]
    [TestCase("it")]
    [TestCase("rm")]
    public void Every_language_the_index_carries_is_honoured(string language)
    {
        SearchController.Language(language).Should().Be(language);
    }

    [TestCase("DE", "de")]
    [TestCase("Fr", "fr")]
    [TestCase(" en ", "en")]
    public void A_language_is_recognised_whatever_its_casing_or_padding(string given, string expected)
    {
        // Query strings arrive however the caller typed them, and the field names in the mapping are
        // lower case.
        SearchController.Language(given).Should().Be(expected);
    }

    [TestCase("xx")]
    [TestCase("de-CH")]
    [TestCase("german")]
    public void A_language_the_index_does_not_carry_falls_back_to_german(string language)
    {
        // Elasticsearch does not object to a field that does not exist, it just matches nothing — so
        // passing the value through would answer a typo with an empty result rather than a wrong one.
        SearchController.Language(language).Should().Be(IndexLanguages.Default);
    }
}
