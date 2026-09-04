using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Tools;

[TestFixture(TestOf = typeof(IdentifierGenerator))]
internal sealed class IdentifierGeneratorTests
{
    [TestCase("Energieträger und Zuordnung gemäss Herkunftßnachweis und Stromkennzeichnung Verordnung (HKSV)")]
    [TestCase("Energieträger? ué äà'k")]
    [TestCase("jdld&5% ddlkn-- a//suh")]
    [TestCase("djjjçéöèüàä/*-+")]
    [TestCase("dnifoed:)")]
    [TestCase("dnifoed :)")]
    public void Given_model_When_GenerateIdentifier_Then_generate_a_correct_identifier(string text)
    {
        // Arrange
        using var dbContext = TestHelper.CreateFakeIopDbContext();

        var subject = new IdentifierGenerator(dbContext);

        var model = new MultiLanguageModel()
        {
            De = text
        };

        // Act
        var result = subject.GenerateIdentifier<Dataset>(model);

        // Assert
        using var _ = new AssertionScope();
        result.Length.Should().BeLessThan(81);
        result.IsValidIdentifier().Should().Be(true);

        dbContext.Database.EnsureDeleted();
    }

    [TestCase("toto", "", null!, "", "", "de")]
    [TestCase(null!, "toto", null!, "", "", "fr")]
    [TestCase(" ", "", "toto", "", "", "it")]
    [TestCase(" ", "", null!, "toto", "", "en")]
    [TestCase(" ", "", null!, "", "toto", "rm")]

    public void Given_MultilanguageModel_When_GenerateIdentifier_Then_get_identifier_from_correct_text_fallback(
        string? de,
        string? fr,
        string? it,
        string? en,
        string? rm,
        string? expectedLanguage)
    {
        // Arrange
        using var dbContext = TestHelper.CreateFakeIopDbContext();

        var subject = new IdentifierGenerator(dbContext);

        var model = ModelsHelper.MultiLanguageModel with
        {
            De = de,
            En = en,
            Fr = fr,
            It = it,
            Rm = rm,
        };

        // Act
        var result = subject.GenerateIdentifier<Dataset>(model);

        // Assert
        result.Should().Be(model.ToDictionary()[expectedLanguage]);

        dbContext.Database.EnsureDeleted();
    }

    [Test]
    public void Given_model_with_existing_identifier_in_db_When_GenerateIdenfier_Then_get_unique_identifier_from_conflict()
    {
        // Arrange
        using var dbContext = TestHelper.CreateFakeIopDbContext();

        var entity = EntitiesHelper.MappingTable;
        entity.Identifiers = [
            "itmustbeabigbigbigbigbigbigbigidentifierbutiamoutofideassoiwritesomethingstupid",
            "itmustbeabigbigbigbigbigbigbigidentifierbutiamoutofideassoiwritesomethingstupi-1"
            ];

        dbContext.MappingTables.Add(entity);
        dbContext.SaveChanges();

        var subject = new IdentifierGenerator(dbContext);

        var model = new MultiLanguageModel()
        {
            De = entity.Identifiers.First()
        };

        // Act
        var result = subject.GenerateIdentifier<MappingTable>(model);

        // Assert
        using var _ = new AssertionScope();
        result.Length.Should().BeLessThan(81);
        result.Should().Be("itmustbeabigbigbigbigbigbigbigidentifierbutiamoutofideassoiwritesomethingstupi-2");
        dbContext.Database.EnsureDeleted();
    }
}
