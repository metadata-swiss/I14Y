using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using Bfs.Iop.DataAccess.Vocabularies;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(ChannelInputModelValidator))]
internal sealed class ChannelInputModelValidatorTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void OnSetup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public void OnTearDown()
    {
        _dbContext?.Database.EnsureDeleted();
        _dbContext?.Dispose();
    }

    [TestCase(ChannelTypesVocabulary.Codes.EmailCode, nameof(ChannelInputModel.Email))]
    [TestCase(ChannelTypesVocabulary.Codes.FaxCode, nameof(ChannelInputModel.Fax))]
    [TestCase(ChannelTypesVocabulary.Codes.MobilePhoneCode, nameof(ChannelInputModel.Mobile))]
    [TestCase(ChannelTypesVocabulary.Codes.PhoneCode, nameof(ChannelInputModel.Phone))]
    [TestCase(ChannelTypesVocabulary.Codes.PostCode, nameof(ChannelInputModel.Address))]
    [TestCase(ChannelTypesVocabulary.Codes.WebCode, nameof(ChannelInputModel.Url))]
    public void Given_ModelWithChannelTypeAndAllNullProperties_When_Validating_Then_OneErrorInSpecificPropertyFound(
        string channelType, 
        string errorPropertyName)
    {
        // Arrange
        var model = ModelsHelper.ChannelInputModel with
        {
            Type = new CodeInputModel()
            {
                Code = channelType,
            },
            Address = null,
            Email = null,
            Fax = null,
            Mobile = null,
            Phone = null,
            Url = null
        };

        var validator = new ChannelInputModelValidator(
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ChannelTypesVocabulary>(),
            _dbContext);

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors.Single().PropertyName.Should().Be(errorPropertyName);
    }

    [TestCase(ChannelTypesVocabulary.Codes.EmailCode)]
    [TestCase(ChannelTypesVocabulary.Codes.FaxCode)]
    [TestCase(ChannelTypesVocabulary.Codes.MobilePhoneCode)]
    [TestCase(ChannelTypesVocabulary.Codes.PhoneCode)]
    [TestCase(ChannelTypesVocabulary.Codes.PostCode)]
    [TestCase(ChannelTypesVocabulary.Codes.WebCode)]
    public void Given_ModelWithChannelTypeAndOnlySpecificPropertyFilled_When_Validating_Then_NoErrorFound(
        string channelType)
    {
        // Arrange
        var model = ModelsHelper.ChannelInputModel with
        {
            Type = new CodeInputModel()
            {
                Code = channelType,
            },
            Address = channelType == ChannelTypesVocabulary.Codes.PostCode
                ? ModelsHelper.MultiLanguageModel
                : null,
            Email = channelType == ChannelTypesVocabulary.Codes.EmailCode
                ? "abc@bfs.admin.ch"
                : null,
            Fax = channelType == ChannelTypesVocabulary.Codes.FaxCode
                ? "some_fax"
                : null,
            Mobile = channelType == ChannelTypesVocabulary.Codes.MobilePhoneCode
                ? "0123456789"
                : null,
            Phone = channelType == ChannelTypesVocabulary.Codes.PhoneCode
                ? "0123456789"
                : null,
            Url = channelType == ChannelTypesVocabulary.Codes.WebCode
                ? "https://input.i14y.admin.ch/"
                : null
        };

        var validator = new ChannelInputModelValidator(
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ChannelTypesVocabulary>(),
            _dbContext);

        // Act
        var result = validator.Validate(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_ModelWithNullChannelType_When_Validating_Then_NoErrorsInSpecificPropertiesFound()
    {
        // Arrange
        var model = ModelsHelper.ChannelInputModel with
        {
            Type = null,
            Address = null,
            Email = null,
            Fax = null,
            Mobile = null,
            Phone = null,
            Url = null
        };

        var validator = new ChannelInputModelValidator(
            TestHelper.CreateFakeVocabularyEntryCodeValidatorWithoutFailures<ChannelTypesVocabulary>(),
            _dbContext);

        // Act
        var result = validator.Validate(model);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeTrue();
    }
}
