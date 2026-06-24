using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.UnitTests.Helpers;
using FluentValidation;
using NSubstitute;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.UnitTests.Services;

[TestFixture(TestOf = typeof(DatasetsService))]
internal sealed class DatasetsServiceTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void Setup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public async Task Teardown()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Test]
    public async Task Given_dataset_id_When_DeleteDataset_Then_delete_it()
    {
        // Arrange
        var entity = EntitiesHelper.Dataset;
        entity.Distributions.Add(EntitiesHelper.Distribution);
        entity.QualifiedAttribution.Add(EntitiesHelper.QualifiedAttribution);
        entity.QualifiedRelation.Add(EntitiesHelper.QualifiedRelation);

        _dbContext.Datasets.Add(entity);

        await _dbContext.SaveChangesAsync();

        var subject = CreateService(_dbContext);

        // Act
        var action = async () => await subject.DeleteDataset(entity.Id, default);

        // Assert
        using var _ = new AssertionScope();
        await action.Should().NotThrowAsync();
        _dbContext.Distributions.Count().Should().Be(0);
        _dbContext.QualifiedAttributions.Count().Should().Be(0);
        _dbContext.QualifiedRelations.Count().Should().Be(0);
        _dbContext.Datasets.Count().Should().Be(0);
    }

    [Test]
    public async Task Given_dataset_id_with_publicService_relation_When_DeleteDataset_Then_throw()
    {
        // Arrange
        var entity = EntitiesHelper.Dataset;
        var publicService = EntitiesHelper.PublicService;
        var relation = new PublicServiceIsDescribedAt()
        {
            IsDescribedAtId = entity.Id,
            PublicServiceId = publicService.Id,
        };

        _dbContext.Datasets.Add(entity);
        _dbContext.PublicServices.Add(publicService);
        _dbContext.PublicServiceDescribedAtDatasetRelations.Add(relation);

        await _dbContext.SaveChangesAsync();

        var subject = CreateService(_dbContext);

        // Act
        var action = async () => await subject.DeleteDataset(entity.Id, default);

        // Assert
        await action.Should().ThrowExactlyAsync<MethodNotAllowedException>();
    }

    [Test]
    public async Task Given_dataset_id_with_dataService_relation_When_DeleteDataset_Then_throw()
    {
        // Arrange
        var entity = EntitiesHelper.Dataset;
        var dataService = EntitiesHelper.DataService;
        var relation = new DataServiceDataset()
        {
            DatasetId = entity.Id,
            DataServiceId = dataService.Id,
        };

        _dbContext.Datasets.Add(entity);
        _dbContext.DataServices.Add(dataService);
        _dbContext.DataServiceDatasetRelations.Add(relation);

        await _dbContext.SaveChangesAsync();

        var subject = CreateService(_dbContext);

        // Act
        var action = async () => await subject.DeleteDataset(entity.Id, default);

        // Assert
        await action.Should().ThrowExactlyAsync<MethodNotAllowedException>();
    }

    [Test]
    public async Task Given_dataset_id_that_is_previous_version_When_DeleteDataset_Then_throw()
    {
        // Arrange
        var entity = EntitiesHelper.Dataset;
        var nextVersion = EntitiesHelper.Dataset;
        nextVersion.PreviousVersionId = entity.Id;

        _dbContext.Datasets.Add(entity);
        _dbContext.Datasets.Add(nextVersion);

        await _dbContext.SaveChangesAsync();

        var subject = CreateService(_dbContext);

        // Act
        var action = async () => await subject.DeleteDataset(entity.Id, default);

        // Assert
        await action.Should().ThrowExactlyAsync<MethodNotAllowedException>();
    }

    private static DatasetsService CreateService(
        IopDbContext dbContext)
    {
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        var registrationStatusPolicyService = Substitute.For<IRegistrationStatusPolicyService>();

        var publicationLevelPolicyService = Substitute.For<IPublicationLevelPolicyService>();

        var publishableEntityAuthorizationService = new PublishableEntityAuthorizationService(
            userContextService,
            publicationLevelPolicyService,
            registrationStatusPolicyService);

        return new(
            dbContext,
            Substitute.For<IAgentsService>(),
            Substitute.For<ICatalogIndexService>(),
            Substitute.For<IIopPersonsService>(),
            new InlineValidator<DcatDatasetInputModel>(),
            new InlineValidator<DatasetQualityInformationDataModel>(),
            userContextService,
            Substitute.For<IVocabulariesService>(),
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            publishableEntityAuthorizationService,
            Substitute.For<IIdentifierGenerator>());
    }
}
