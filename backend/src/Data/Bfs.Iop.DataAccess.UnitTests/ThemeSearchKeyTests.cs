using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.UnitTests;

[TestFixture(TestOf = typeof(ThemeSearchKey))]
internal sealed class ThemeSearchKeyTests
{
    [Test]
    public void Given_a_theme_of_a_vocabulary_When_building_its_key_Then_both_parts_are_joined()
    {
        // Arrange
        var theme = new VocabularyEntryModel { Code = "AGRI", VocabularyIdentifier = "VOCAB_EU_DATA_THEME" };

        // Act & Assert
        ThemeSearchKey.For(theme).Should().Be("VOCAB_EU_DATA_THEME!AGRI");
    }

    [Test]
    public void Given_a_theme_of_no_vocabulary_When_building_its_key_Then_the_code_alone_is_used()
    {
        // Arrange: a stored value resolving to no registered taxonomy keeps its place in the index.
        var theme = new VocabularyEntryModel { Code = "101" };

        // Act & Assert
        ThemeSearchKey.For(theme).Should().Be("101");
    }

    [Test]
    public void Given_a_key_When_splitting_it_Then_the_vocabulary_and_the_code_come_back()
    {
        // Act
        var parts = ThemeSearchKey.TrySplit("VOCAB_EU_DATA_THEME!AGRI");

        // Assert
        parts.Should().Be(("VOCAB_EU_DATA_THEME", "AGRI"));
    }

    [Test]
    public void Given_a_code_containing_the_separator_When_splitting_its_key_Then_only_the_first_one_splits()
    {
        // Arrange: a vocabulary identifier cannot contain the separator, a code may.
        var theme = new VocabularyEntryModel { Code = "A!B", VocabularyIdentifier = "VOCAB" };

        // Act
        var parts = ThemeSearchKey.TrySplit(ThemeSearchKey.For(theme));

        // Assert
        parts.Should().Be(("VOCAB", "A!B"));
    }

    [TestCase("101")]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("!AGRI")]
    public void Given_a_value_carrying_no_vocabulary_When_splitting_it_Then_nothing_comes_back(string? value)
    {
        // Act & Assert
        ThemeSearchKey.TrySplit(value).Should().BeNull();
    }
}
