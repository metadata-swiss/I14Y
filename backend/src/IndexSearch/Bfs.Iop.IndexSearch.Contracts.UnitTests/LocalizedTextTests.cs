using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;

namespace Bfs.Iop.IndexSearch.Contracts.UnitTests;

[TestFixture(TestOf = typeof(LocalizedText))]
public class LocalizedTextTests
{
    [Test]
    public void ToDictionary_keeps_only_populated_languages()
    {
        var text = new LocalizedText { De = "Bevoelkerung", Fr = "Population" };

        text.ToDictionary().Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["de"] = "Bevoelkerung",
            ["fr"] = "Population",
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ToDictionary_drops_blank_languages(string blank)
    {
        var text = new LocalizedText { De = "Bevoelkerung", En = blank };

        text.ToDictionary().Should().ContainKey("de").And.NotContainKey("en");
    }

    [Test]
    public void IsEmpty_is_true_only_when_nothing_is_populated()
    {
        new LocalizedText().IsEmpty.Should().BeTrue();
        new LocalizedText { De = "  " }.IsEmpty.Should().BeTrue();
        new LocalizedText { Rm = "Populaziun" }.IsEmpty.Should().BeFalse();
    }
}
