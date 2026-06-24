using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(IopConceptInputModelValidator))]
internal sealed class IopConceptInputModelValidatorTests
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
    public void Given_conceptType_CodeList_with_invalid_properties_When_validating_Then_Fails()
    {
        // Arrange
        var fakeModel = ModelsHelper.IopConceptInputModel with
        { 
            ConceptType = ConceptType.CodeList ,
            CodeListEntryValueMaxLength = null!,
            CodeListEntryValueType = null!,
        };

        var subject = CreateFakeValidator(_dbContext);

        // Act
        var result = subject.Validate(fakeModel);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Any(x => x.PropertyName == nameof(IopConceptInputModel.CodeListEntryValueMaxLength)).Should().Be(true);
        result.Errors.Any(x => x.PropertyName == nameof(IopConceptInputModel.CodeListEntryValueType)).Should().Be(true);
    }

    [TestCase(null!, null! , "")]
    [TestCase(-5, 2, "toto")]
    [TestCase(2, 1, "toto")]
    [TestCase(0, 2, " ")]
    public void Given_conceptType_String_with_invalid_properties_When_validating_Then_Fails(
        int? minLength,
        int? maxLength, 
        string pattern)
    {
        // Arrange
        var fakeModel = ModelsHelper.IopConceptInputModel with
        {
            ConceptType = ConceptType.String,
            MinLength = minLength,
            MaxLength = maxLength,
            Pattern = pattern
        };

        var subject = CreateFakeValidator(_dbContext);

        // Act
        var result = subject.Validate(fakeModel);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Any(x => 
            x.PropertyName == nameof(IopConceptInputModel.MinLength) || 
            x.PropertyName == nameof(IopConceptInputModel.MaxLength) ||
            x.PropertyName == nameof(IopConceptInputModel.Pattern))
            .Should().Be(true);
    }

    [TestCase(null!, null!, null!, "")]
    [TestCase(10.3, 2.78, 5, "toto")]
    [TestCase(2.69, 10.23, -5, "toto")]
    [TestCase(2.076, -10.908, 5, "toto")]
    [TestCase(-2.56, 10.123, 5, " ")]
    public void Given_conceptType_Numeric_with_invalid_properties_When_validating_Then_Fails(
        decimal? minValue, decimal? maxValue, int? numberDecimals, string pattern)
    {
        // Arrange
        var fakeModel = ModelsHelper.IopConceptInputModel with
        {
            ConceptType = ConceptType.Numeric,
            MinValue = minValue,
            MaxValue = maxValue,
            NumberDecimals = numberDecimals,
            Pattern = pattern
        };

        var subject = CreateFakeValidator(_dbContext);

        // Act
        var result = subject.Validate(fakeModel);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Any(x => 
            x.PropertyName == nameof(IopConceptInputModel.MinValue) ||
            x.PropertyName == nameof(IopConceptInputModel.MaxValue) ||
            x.PropertyName == nameof(IopConceptInputModel.NumberDecimals) ||
            x.PropertyName == nameof(IopConceptInputModel.Pattern))
            .Should().Be(true);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void Given_conceptType_Date_with_invalid_properties_When_validating_Then_Fails(string pattern)
    {
        // Arrange
        var fakeModel = ModelsHelper.IopConceptInputModel with
        {
            ConceptType = ConceptType.Date,
            Pattern = pattern
        };

        var subject = CreateFakeValidator(_dbContext);

        // Act
        var result = subject.Validate(fakeModel);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Any(x => x.PropertyName == nameof(IopConceptInputModel.Pattern)).Should().Be(true);
    }

    [Test]
    public void Given_iopConceptInputModel_with_Identifier_and_Version_from_another_entity_When_Validating_Then_fails()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            PublisherId = EntitiesHelper.Agent.Id,
            Version = fakeVersion
        };

        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.SaveChanges();

        var subject = CreateFakeValidator(_dbContext);

        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Count.Should().Be(1);
        result.Errors.First().PropertyName.Should().Be($"{nameof(model.Identifiers)}[0]");
    }

    [Test]
    public void Given_iopConceptInputModel_with_same_Identifier_and_Version_from_existing_entity_When_Validating_Then_fails()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var id = Guid.NewGuid();

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            PublisherId = EntitiesHelper.Agent.Id,
            Version = fakeVersion,
            Id = id
        };

        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.SaveChanges();

        var subject = CreateFakeValidator(_dbContext);
        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.First().PropertyName.Should().Be($"{nameof(model.Identifiers)}[0]");
    }

    [Test]
    public void Given_iopConceptInputModel_with_same_Identifier_and_Version_from_existing_entity_with_same_id_When_Validating_Then_ok()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var id = Guid.NewGuid();

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            PublisherId = EntitiesHelper.Agent.Id,
            Version = fakeVersion,
            Id = id
        };

        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.SaveChanges();

        var subject = CreateFakeValidator(_dbContext);
        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion
        };

        var context = new ValidationContext<IopConceptInputModel>(model);
        context.RootContextData.Add(ValidationContextDataKeys.IdKey, id);
        
        // Act
        var result = subject.Validate(context);

        // Assert
        result.IsValid.Should().Be(true);
    }

    [Test]
    public void Given_iopConceptVersionInputModel_with_Identifier_of_existing_entity_and_new_Version_When_Validating_Then_ok()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var fakeVersionNew = "0.0.2";

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion
        };

        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.SaveChanges();

        var subject = CreateFakeValidator(_dbContext);
        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersionNew
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_input_model_with_same_identifier_and_different_version_as_entity_with_different_publisher_When_validating_Then_validation_fails()
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var entity = EntitiesHelper.IopConcept;

        var agent = new Agent()
        {
            Id = Guid.NewGuid(),
            Identifier = "test_agent"
        };

        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [entity.Identifiers.First()],
            Version = "2.0.0",
            Publisher = new() { Identifier = agent.Identifier },
        };

        _dbContext.Agents.Add(agent);
        _dbContext.IopConcepts.Add(entity);
        _dbContext.SaveChanges();

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Single().ErrorMessage.Should().Be($"The identifier '{model.Identifiers.First()}' already exists by another publisher.");
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
        var model = ModelsHelper.IopConceptInputModel with
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

    [TestCase(true)]
    [TestCase(false)]
    public void Given_input_model_with_null_validFrom_When_validating_Then_validation_ok(bool validToIsSet)
    {
        // Arrange
        var subject = CreateFakeValidator(_dbContext);

        var model = ModelsHelper.IopConceptInputModel with
        {
            ValidFrom = null,
            ValidTo = validToIsSet ? new DateTimeOffset(2026, 5, 26, 0, 0, 0, TimeSpan.Zero) : null
        };

        // Act
        var result = subject.Validate(model);

        // Assert
        result.IsValid.Should().Be(true);
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

        var model = ModelsHelper.IopConceptInputModel with
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

    private static IopConceptInputModelValidator CreateFakeValidator(IopDbContext dbContext)
    {
        var themesValidator = TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ThemesVocabulary>();
        var resourceInputModelValidator = new InlineValidator<ResourceModel>();
        var keywordValidator = new InlineValidator<KeywordModel>();

        return new(
            themesValidator,
            resourceInputModelValidator,
            keywordValidator,
            dbContext);
    }
}
