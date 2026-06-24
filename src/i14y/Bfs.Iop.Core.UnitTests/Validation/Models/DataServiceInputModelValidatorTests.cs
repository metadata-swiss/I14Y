using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(DataServiceInputModelValidator))]
internal sealed class DataServiceInputModelValidatorTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void Setup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public void OnTearDown()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }

    [Test]
    public void Given_model_with_mandatory_fields_When_validate_Then_Ok()
    {
        // Arrange
        var model = ModelsHelper.DataServiceInputModel;

        var subject = new DataServiceInputModelValidator(
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<RightsStatementsVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<LicenseTypesVocabulary>(),
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ThemesVocabulary>(),
            new InlineValidator<ResourceModel>(),
            new InlineValidator<VCardModel>(),
            new InlineValidator<KeywordModel>(),
            _dbContext!);

        // Act
        var result = subject.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.Errors.Should().BeEmpty();
        result.IsValid.Should().Be(true);
    }
}
