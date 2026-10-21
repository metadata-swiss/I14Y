using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Contracts;
using FluentValidation;
using NSubstitute;
using Bfs.Iop.Core.UnitTests.Helpers;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

// An agent's identifier is a key two other systems join on: it is denormalised into the search index
// as publisherIdentifier, and it is matched against the agency claims in a caller's token. Changing
// it detaches every resource the agent publishes from both at once, and neither notices.
[TestFixture(TestOf = typeof(AgentsService))]
internal sealed class AgentsServiceTests
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
    public async Task Given_a_changed_identifier_When_UpdateAgent_Then_reject_it()
    {
        var entity = await SeedAgentAsync("organisation-a");

        var action = async () => await CreateService().UpdateAgent(entity.Id, Input("organisation-b"), default);

        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(x =>
                x.PropertyName == nameof(AgentInputModel.Identifier) &&
                x.ErrorMessage.Contains("cannot be changed"));
    }

    [Test]
    public async Task Given_a_changed_identifier_When_UpdateAgent_Then_leave_the_stored_one_alone()
    {
        var entity = await SeedAgentAsync("organisation-a");

        try
        {
            await CreateService().UpdateAgent(entity.Id, Input("organisation-b"), default);
        }
        catch (ValidationException)
        {
            // The assertion is what reached the database, not that it threw.
        }

        _dbContext.ChangeTracker.Clear();
        _dbContext.Agents.Single(x => x.Id == entity.Id).Identifier.Should().Be("organisation-a");
    }

    [Test]
    public async Task Given_the_same_identifier_When_UpdateAgent_Then_allow_the_rest_of_the_update()
    {
        // Every PUT carries the identifier because the input model requires it, so an ordinary edit
        // resends the current value. Rejecting that would block all agent updates.
        var entity = await SeedAgentAsync("organisation-a");

        var input = Input("organisation-a") with { HomePage = "https://example.ch" };

        var action = async () => await CreateService().UpdateAgent(entity.Id, input, default);

        await action.Should().NotThrowAsync();

        _dbContext.ChangeTracker.Clear();
        _dbContext.Agents.Single(x => x.Id == entity.Id).HomePage.Should().Be("https://example.ch");
    }

    [Test]
    public async Task Given_only_a_change_of_case_When_UpdateAgent_Then_reject_it_too()
    {
        // The index lowercases both the stored value and the caller's agencies, so it would not see
        // this one - but UserBelongsToAgency compares the claim verbatim, so Core's write
        // authorization would break. The comparison has to stay ordinal.
        var entity = await SeedAgentAsync("organisation-a");

        var action = async () => await CreateService().UpdateAgent(entity.Id, Input("Organisation-A"), default);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task Given_an_identifier_another_agent_holds_When_UpdateAgent_Then_report_both_reasons()
    {
        // Asserting on the messages, not merely that it threw: the immutability guard rejects this
        // input too, so a test that only expects a ValidationException passes with the uniqueness
        // rule deleted.
        var entity = await SeedAgentAsync("organisation-a");
        await SeedAgentAsync("organisation-b");

        var action = async () => await CreateService().UpdateAgent(entity.Id, Input("organisation-b"), default);

        var messages = (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Select(x => x.ErrorMessage);

        messages.Should().Contain(x => x.Contains("cannot be changed"))
            .And.Contain(x => x.Contains("same identifier already exists"));
    }

    [Test]
    public async Task Given_an_identifier_another_agent_holds_When_AddAgent_Then_reject_it()
    {
        // Create is the only path where uniqueness can still fail on its own, now that an update
        // cannot change the identifier at all.
        await SeedAgentAsync("organisation-a");

        var action = async () => await CreateService().AddAgent(Input("organisation-a"), default);

        (await action.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(x => x.ErrorMessage.Contains("same identifier already exists"));
    }

    [Test]
    public async Task Given_a_fresh_identifier_When_AddAgent_Then_accept_it()
    {
        // The guard only applies to an update; creation still assigns an identifier freely.
        var action = async () => await CreateService().AddAgent(Input("organisation-new"), default);

        await action.Should().NotThrowAsync();
        _dbContext.Agents.Any(x => x.Identifier == "organisation-new").Should().BeTrue();
    }

    private async Task<Agent> SeedAgentAsync(string identifier)
    {
        var entity = EntitiesHelper.Agent;
        entity.Id = Guid.NewGuid();
        entity.Identifier = identifier;

        _dbContext.Agents.Add(entity);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        return entity;
    }

    private static AgentInputModel Input(string identifier) => new()
    {
        ContactPoint = ModelsHelper.VCardModel,
        Identifier = identifier,
        Name = new MultiLanguageModel { De = "Organisation" },
        PrefLabel = new MultiLanguageModel { De = "Organisation" },
    };

    private AgentsService CreateService()
    {
        var userContextService = TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser();

        return new(
            _dbContext,
            Substitute.For<IVocabulariesService>(),
            new EntityAuthorizationService(userContextService),
            userContextService,
            new InlineValidator<AgentInputModel>());
    }
}
