using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(MappingTableInputModelValidator))]
internal sealed class MappingTableInputModelValidatorTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void Setup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public void Given_input_model_with_same_identifier_and_version_as_entity_When_validating_Then_validation_fails()
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var entity = EntitiesHelper.MappingTable;

        var model = ModelsHelper.MappingTableInputModel with
        {
            Identifiers = [entity.Identifiers.First()],
            Version = entity.Version
        };

        _dbContext.MappingTables.Add(entity);
        _dbContext.SaveChanges();

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Single().ErrorMessage.Should().Be($"The identifier '{model.Identifiers.Single()}' with version '{model.Version}' is already in use.");
    }

    [Test]
    public void Given_input_model_with_same_identifier_and_different_version_as_entity_When_validating_Then_validation_ok()
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var entity = EntitiesHelper.MappingTable;

        var model = ModelsHelper.MappingTableInputModel with
        {
            Identifiers = [entity.Identifiers.First()],
            Version = "2.0.0"
        };

        _dbContext.MappingTables.Add(entity);
        _dbContext.SaveChanges();

        // Act
        var result = subject.Validate(model);

        // Assert
        result.IsValid.Should().Be(true);
    }

    [TestCase(-1, false, "ValidTo must not be earlier than ValidFrom.")]
    [TestCase(0, true, null)]
    [TestCase(1, true, null)]
    public void Given_input_model_with_validTo_relative_to_validFrom_When_validating_Then_validates_accordingly(
        int validToDayOffset, bool expectedValid, string? expectedErrorMessage)
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var validFrom = new DateTimeOffset(2026, 5, 26, 0, 0, 0, TimeSpan.Zero);
        var model = ModelsHelper.MappingTableInputModel with
        {
            ValidFrom = validFrom,
            ValidTo = validFrom.AddDays(validToDayOffset)
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(expectedValid);
        if (!expectedValid)
        {
            result.Errors.Single().ErrorMessage.Should().Be(expectedErrorMessage);
        }
    }

    [Test]
    public void Given_input_model_with_same_identifier_and_different_version_as_entity_with_different_publisher_When_validating_Then_validation_fails()
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var entity = EntitiesHelper.MappingTable;

        var agent = new Agent()
        {
            Id = Guid.NewGuid(),
            Identifier = "test_agent"
        };

        var model = ModelsHelper.MappingTableInputModel with
        {
            Identifiers = [entity.Identifiers.First()],
            Version = "2.0.0",
            Publisher = new() { Identifier = agent.Identifier },
        };

        _dbContext.Agents.Add(agent);
        _dbContext.MappingTables.Add(entity);
        _dbContext.SaveChanges();

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Single().ErrorMessage.Should().Be($"The identifier '{model.Identifiers.First()}' already exists by another publisher.");
    }

    [TestCase("toto", "tata", true)]
    [TestCase(null, null, false)]
    [TestCase(null, "tata", false)]
    [TestCase("toto", null, true)]
    [TestCase("toto", "TOTO", false)]
    public void Given_inputModel_with_responsibles_When_validating_Then_expected(
        string? responsibleIdentifier,
        string? deputyIdentifier,
        bool expectedResult)
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var model = ModelsHelper.MappingTableInputModel with
        {
            ResponsiblePerson = responsibleIdentifier is not null
                ? new EmailInputModel() { Email = responsibleIdentifier }
                : null!,

            ResponsibleDeputy = deputyIdentifier is not null
                ? new EmailInputModel() { Email = deputyIdentifier }
                : null!,
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        result.IsValid.Should().Be(expectedResult);
    }

    private static MappingTableInputModelValidator CreateFakeValidator(IopDbContext dbContext) =>
        new(dbContext,
            new InlineValidator<ResourceModel>(),
            new InlineValidator<KeywordModel>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ThemesVocabulary>());
}
