using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using FluentValidation;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(DcatCatalogRecordInputModelValidator))]
internal sealed class DcatCatalogRecordInputModelValidatorTests
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
    public void Given_model_with_primary_topic_When_validating_Then_should_be_ok()
    {
        // Arrange
        var catalog = EntitiesHelper.DcatCatalog;
        var dataService = EntitiesHelper.DataService;

        _dbContext.DataServices.Add(dataService);
        _dbContext.DcatCatalogs.Add(catalog);
        _dbContext.SaveChanges();

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            }
        };

        var subject = CreateFakeValidator(_dbContext);
        var context = new ValidationContext<DcatCatalogRecordInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey] = catalog;
        context.RootContextData[ValidationContextDataKeys.IdKey] = null;

        // Act
        var result = subject.Validate(context);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_model_with_already_existing_primary_topic_When_validating_Then_should_throw_error()
    {
        // Arrange
        var catalog = EntitiesHelper.DcatCatalog;
        var dataService = EntitiesHelper.DataService;

        _dbContext.DataServices.Add(dataService);
        _dbContext.DcatCatalogs.Add(catalog);

        var record = new DcatCatalogRecord()
        {
            DcatCatalogId = catalog.Id,
            PrimaryTopic = new DcatCatalogResource()
            {
                ResourceId = dataService.Id,
                ResourceType = "DataService"
            },
        };

        _dbContext.DcatCatalogRecords.Add(record);
        _dbContext.SaveChanges();

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            }
        };

        var subject = CreateFakeValidator(_dbContext);
        var context = new ValidationContext<DcatCatalogRecordInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey] = catalog;
        context.RootContextData[ValidationContextDataKeys.IdKey] = null;

        // Act
        var result = subject.Validate(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.First().ErrorMessage.Should().Be($"A catalog record for the resource with the id '{model.PrimaryTopic.ResourceId}' already exists in the catalog.");
    }

    [Test]
    public void Given_model_with_same_id_and_with_already_existing_primary_topic_When_validating_Then_should_be_ok()
    {
        // Arrange
        var catalog = EntitiesHelper.DcatCatalog;
        var dataService = EntitiesHelper.DataService;

        _dbContext.DataServices.Add(dataService);
        _dbContext.DcatCatalogs.Add(catalog);

        var record = new DcatCatalogRecord()
        {
            Id = Guid.NewGuid(),
            DcatCatalogId = catalog.Id,
            PrimaryTopic = new DcatCatalogResource()
            {
                ResourceId = dataService.Id,
                ResourceType = "DataService"
            },
        };

        _dbContext.DcatCatalogRecords.Add(record);
        _dbContext.SaveChanges();

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            }
        };

        var subject = CreateFakeValidator(_dbContext);
        var context = new ValidationContext<DcatCatalogRecordInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey] = catalog;
        context.RootContextData[ValidationContextDataKeys.IdKey] = record.Id;

        // Act
        var result = subject.Validate(context);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_model_with_duplicated_codes_of_same_themeTaxonomy_When_validating_Then_throw_error()
    {
        // Arrange
        var catalog = EntitiesHelper.DcatCatalog;
        var dataService = EntitiesHelper.DataService;

        _dbContext.DataServices.Add(dataService);
        _dbContext.DcatCatalogs.Add(catalog);
        _dbContext.SaveChanges();

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            },
            Themes = 
            [
                new() 
                { 
                    Code = "100",
                    ThemeTaxonomy = "Taxonomy1"        
                },
                new()
                {
                    Code = "100",
                    ThemeTaxonomy = "Taxonomy1"
                },
            ]
        };

        var subject = CreateFakeValidator(_dbContext);
        var context = new ValidationContext<DcatCatalogRecordInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey] = catalog;
        context.RootContextData[ValidationContextDataKeys.IdKey] = null;

        // Act
        var result = subject.Validate(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.ErrorMessage == "The collection cannot contain repeated codes.").Should().BeTrue();
    }

    [Test]
    public void Given_model_with_duplicated_codes_of_different_themeTaxonomy_When_validating_Then_throw_error()
    {
        // Arrange
        var catalog = EntitiesHelper.DcatCatalog;
        var dataService = EntitiesHelper.DataService;

        _dbContext.DataServices.Add(dataService);
        _dbContext.DcatCatalogs.Add(catalog);
        _dbContext.SaveChanges();

        var model = new DcatCatalogRecordInputModel()
        {
            PrimaryTopic = new DcatCatalogResourceModel()
            {
                ResourceId = dataService.Id,
                ResourceType = DcatCatalogType.DataService
            },
            Themes =
            [
                new()
                {
                    Code = "100",
                    ThemeTaxonomy = "Taxonomy1"
                },
                new()
                {
                    Code = "100",
                    ThemeTaxonomy = "Taxonomy2"
                },
            ]
        };

        var subject = CreateFakeValidator(_dbContext);
        var context = new ValidationContext<DcatCatalogRecordInputModel>(model);
        context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey] = catalog;
        context.RootContextData[ValidationContextDataKeys.IdKey] = null;

        // Act
        var result = subject.Validate(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().BeFalse();
        result.Errors.Any(x => x.ErrorMessage == "The collection cannot contain repeated codes.").Should().BeFalse();
    }

    private static DcatCatalogRecordInputModelValidator CreateFakeValidator(IopDbContext dbContext) => 
        new(dbContext, NSubstitute.Substitute.For<IVocabulariesService>());
}
