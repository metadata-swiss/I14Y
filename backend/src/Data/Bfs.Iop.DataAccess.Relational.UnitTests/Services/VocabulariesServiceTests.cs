using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

internal sealed class  VocabulariesServiceTests
{
    private IopDbContext _dbContext = null!;
    private IEntityAuthorizationService _entityAuthorizationService = null!;
    private IUserContextService _userContextService = null!;

    [SetUp]
    public void Setup()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();
        _entityAuthorizationService = Substitute.For<IEntityAuthorizationService>();
        _userContextService = Substitute.For<IUserContextService>();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task GetVocabularyConfig_WhenConfigExists_ShouldReturnCorrectConfig()
    {
        // Arrange
        var vocabulariesService = new VocabulariesService(
            _dbContext,
            _entityAuthorizationService,
            _userContextService);

        var config = EntitiesHelper.VocabularyConfig;
        await _dbContext.VocabularyConfigs.AddAsync(config);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await vocabulariesService.GetVocabularyConfig(config.Id, CancellationToken.None);

        // Assert
        using var _ = new AssertionScope();    
        result.Should().NotBeNull();
        result.Id.Should().Be(config.Id);
        result.VocabularyIdentifier.Should().Be(config.VocabularyIdentifier);
        result.ConceptIdentifier.Should().Be(config.ConceptIdentifier);
        result.ConceptVersion.Should().Be(config.ConceptVersion);     
    }

    [Test]
    public async Task GetVocabularyConfig_WhenConfigDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var vocabulariesService = new VocabulariesService(
            _dbContext,
            _entityAuthorizationService,
            _userContextService);

        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        var action = async () => await vocabulariesService.GetVocabularyConfig(nonExistentId, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task AddVocabularyConfig_ShouldCreateNewConfigAndReturnId()
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        var vocabulariesService = new VocabulariesService(
            _dbContext,
            new EntityAuthorizationService(userContextService),
            userContextService);

        var concept = EntitiesHelper.IopConcept;
        concept.PublicationLevel = PublicationLevel.Public;
        concept.ConceptType = ConceptType.CodeList;
        concept.IsLocked = true;

        var codeListEntry = EntitiesHelper.CodeListEntry;
        codeListEntry.IopConceptId = concept.Id;

        var configModel = ModelsHelper.VocabularyConfigInputModel with
        {
            ConceptVersion = concept.Version,
            ConceptIdentifier = concept.Identifiers.First()
        };

        await _dbContext.IopConcepts.AddAsync(concept);
        await _dbContext.CodeListEntries.AddAsync(codeListEntry);
        await _dbContext.SaveChangesAsync();

        // Act
        var resultId = await vocabulariesService.AddVocabularyConfig(configModel, CancellationToken.None);

        // Assert
        using var _ = new AssertionScope();
        resultId.Should().NotBeEmpty();
        var savedConfig = await _dbContext.VocabularyConfigs.FindAsync(resultId);
        savedConfig.Should().NotBeNull();
        savedConfig!.VocabularyIdentifier.Should().Be(configModel.VocabularyIdentifier);
        savedConfig.ConceptIdentifier.Should().Be(configModel.ConceptIdentifier);
        savedConfig.ConceptVersion.Should().Be(configModel.ConceptVersion);
    }

    [Test]
    public async Task UpdateVocabularyConfig_WhenUpdatingExistingConfig_ShouldUpdate()
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        var vocabulariesService = new VocabulariesService(
            _dbContext,
            new EntityAuthorizationService(userContextService),
            userContextService);

        var concept = EntitiesHelper.IopConcept;
        concept.Identifiers = ["UpdatedConcept"];
        concept.Version = "1.1.0";
        concept.PublicationLevel = PublicationLevel.Public;
        concept.ConceptType = ConceptType.CodeList;
        concept.IsLocked = true;

        var codeListEntry = EntitiesHelper.CodeListEntry;
        codeListEntry.IopConceptId = concept.Id;

        var config = EntitiesHelper.VocabularyConfig;

        await _dbContext.IopConcepts.AddAsync(concept);
        await _dbContext.VocabularyConfigs.AddAsync(config);
        await _dbContext.CodeListEntries.AddAsync(codeListEntry);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var updatedConfigModel = ModelsHelper.VocabularyConfigInputModel with
        {
            VocabularyIdentifier = "UpdatedVocabulary",
            ConceptIdentifier = concept.Identifiers.First(),
            ConceptVersion = concept.Version
        };
        
        // Act
        await vocabulariesService.UpdateVocabularyConfig(config.Id, updatedConfigModel, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            var updatedConfig = await _dbContext.VocabularyConfigs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == config.Id);
            updatedConfig.Should().NotBeNull();
            updatedConfig.VocabularyIdentifier.Should().Be(updatedConfigModel.VocabularyIdentifier);
            updatedConfig.ConceptIdentifier.Should().Be(updatedConfigModel.ConceptIdentifier);
            updatedConfig.ConceptVersion.Should().Be(updatedConfigModel.ConceptVersion);
        }
    }

    [Test]
    public async Task DeleteVocabularyConfig_WhenConfigExists_ShouldRemoveConfig()
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        var vocabulariesService = new VocabulariesService(
            _dbContext,
            new EntityAuthorizationService(userContextService),
            userContextService);

        var config = EntitiesHelper.VocabularyConfig;

        await _dbContext.VocabularyConfigs.AddAsync(config);
        await _dbContext.SaveChangesAsync();

        // Act
        await vocabulariesService.DeleteVocabularyConfig(config.Id, CancellationToken.None);

        // Assert
        var deletedConfig = await _dbContext.VocabularyConfigs.FindAsync(config.Id);
        deletedConfig.Should().BeNull();
    }

    [Test]
    public async Task DeleteVocabularyConfig_WhenConfigDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        var vocabulariesService = new VocabulariesService(
            _dbContext,
            new EntityAuthorizationService(userContextService),
            userContextService);

        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        Func<Task> action = async () => await vocabulariesService.DeleteVocabularyConfig(nonExistentId, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
