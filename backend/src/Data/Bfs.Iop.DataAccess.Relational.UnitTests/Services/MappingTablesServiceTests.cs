using AwesomeAssertions;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Authorization;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

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
            Identifiers = entity.Identifiers.Concat(["jidfejfioe"]).ToList()
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
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            publishableEntityAuthorizationService,
            Substitute.For<IIdentifierGenerator>(),
            userContextService);
    }
}
