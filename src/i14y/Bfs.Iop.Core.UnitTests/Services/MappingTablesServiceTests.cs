using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Services;

[TestFixture(TestOf = typeof(MappingTablesService))]
internal sealed class MappingTablesServiceTests
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
    public void Given_inputModel_with_different_identifiers_When_AddMappingTableVersion_Then_throw_ValidationException()
    {
        // Arrange        
        var entity = EntitiesHelper.MappingTable;

        _dbContext.MappingTables.Add(entity);
        _dbContext.SaveChanges();

        var inputModel = ModelsHelper.MappingTableInputModel with
        {
            Identifiers = ["dhfihjfioe"]
        };

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, EntitiesHelper.Agent.Identifier)
        };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        var subject = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => subject.AddMappingTableVersion(entity.Id, inputModel, cancellationToken: default).GetAwaiter().GetResult();

        // Assert
        action.Should().ThrowExactly<ValidationException>().Which.Errors.First().PropertyName.Should().Be(nameof(inputModel.Identifiers));
    }

    [Test]
    public void Given_inputModel_with_different_number_of_identifiers_When_AddMappingTableVersion_Then_throw_ValidationException()
    {
        // Arrange
        var entity = EntitiesHelper.MappingTable;

        _dbContext.MappingTables.Add(entity);
        _dbContext.SaveChanges();

        var inputModel = ModelsHelper.MappingTableInputModel with
        {
            Identifiers = entity.Identifiers.Concat(["jidfejfioe"])
        };

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, EntitiesHelper.Agent.Identifier)
        };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        var subject = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => subject.AddMappingTableVersion(entity.Id, inputModel, cancellationToken: default).GetAwaiter().GetResult();

        // Assert
        action.Should().ThrowExactly<ValidationException>().Which.Errors.First().PropertyName.Should().Be(nameof(inputModel.Identifiers));
    }

    private static MappingTablesService CreateFakeService(
        IopDbContext dbContext,
        IUserContextService userContextService)
    {
        var registrationStatusPolicyService = Substitute.For<IRegistrationStatusPolicyService>();

        var publicationLevelPolicyService = Substitute.For<IPublicationLevelPolicyService>();

        var publishableEntityAuthorizationService = new PublishableEntityAuthorizationService(
            userContextService,
            publicationLevelPolicyService,
            registrationStatusPolicyService);

        var vocabulariesService = Substitute.For<IVocabulariesService>();

        var agentsService = new AgentsService(
            dbContext,
            vocabulariesService,
            publishableEntityAuthorizationService,
            userContextService,
            new InlineValidator<AgentInputModel>());

        var personsService = new IopPersonsService(dbContext, publishableEntityAuthorizationService, userContextService);

        return new MappingTablesService(
            agentsService,
            personsService,
            vocabulariesService,
            new InlineValidator<MappingTableInputModel>(),
            new InlineValidator<IEnumerable<MappingRelationInputModel>>(),
            dbContext,
            Substitute.For<ICatalogIndexService>(),
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            publishableEntityAuthorizationService,
            Substitute.For<IIdentifierGenerator>(),
            userContextService);
    }
}
