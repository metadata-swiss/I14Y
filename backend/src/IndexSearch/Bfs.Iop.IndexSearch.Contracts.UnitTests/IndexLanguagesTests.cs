using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture(TestOf = typeof(IndexLanguages))]
public class IndexLanguagesTests
{

    [Test]
    public void The_languages_are_the_five_the_index_maps() =>
        IndexLanguages.All.Should().Equal("de", "en", "fr", "it", "rm");

    [Test]
    public void The_default_language_is_german() =>
        IndexLanguages.Default.Should().Be("de");
}
