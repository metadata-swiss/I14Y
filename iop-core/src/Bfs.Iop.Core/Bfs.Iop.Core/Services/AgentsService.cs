using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

internal sealed class AgentsService : AuthorizedEntityServiceBase<Agent>, IAgentsService
{
    private readonly IopDbContext _iopDbContext;
    private readonly IValidator<AgentInputModel> _agentValidator;
    private readonly IVocabulariesService _vocabulariesService;

    protected override IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDeleteEntity =>
    [
        BusinessRole.InteroperabilityService
    ];

    public AgentsService(
        IopDbContext iopDbContext,
        IVocabulariesService vocabulariesService,
        IEntityAuthorizationService entityUserAuthorizationService,
        IUserContextService userContextService,
        IValidator<AgentInputModel> agentValidator)
            : base(entityUserAuthorizationService, userContextService)
    {
        _iopDbContext = iopDbContext ?? throw new ArgumentNullException(nameof(iopDbContext));
        _agentValidator = agentValidator ?? throw new ArgumentNullException(nameof(agentValidator));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }

    public async Task<AgentModel> GetAgent(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: true, entityIncludeLevel: EntityIncludeLevel.All, cancellationToken);

        return entity.MapToAgentModel(_vocabulariesService);
    }

    public async Task<PagedResult<AgentModel>> GetAgents(
        string? identifier,
        string? uid, 
        int page, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        Expression<Func<Agent, bool>> filter = x =>
            (string.IsNullOrWhiteSpace(identifier) || x.Identifier  == identifier) &&
            (string.IsNullOrWhiteSpace(uid) || x.Uid == uid);

        var query = CreateGetAgentsQuery(asNoTracking: true, EntityIncludeLevel.All, filter);

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AgentModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = entities.Select(x => x.MapToAgentModel(_vocabulariesService)),
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<AgentModel>> GetAgents(IEnumerable<string> identifiers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(identifiers, nameof(identifiers));

        var query = CreateGetAgentsQuery(asNoTracking: true, EntityIncludeLevel.All, filter: x => identifiers.Contains(x.Identifier));

        var entities = await query.ToListAsync(cancellationToken);

        return entities.Select(x => x.MapToAgentModel(_vocabulariesService));
    }

    public async Task<IEnumerable<AgentModel>> GetAgents(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));

        var query = CreateGetAgentsQuery(asNoTracking: true, EntityIncludeLevel.All, filter: x => ids.Contains(x.Id));

        var entities = await query.ToListAsync(cancellationToken);

        return entities.Select(x => x.MapToAgentModel(_vocabulariesService));
    }

    public async Task<IEnumerable<AgentModel>> GetAgentParentAgents(Guid id, CancellationToken cancellationToken = default)
    {
        var relations = await _iopDbContext.AgentSubAgentRelations
            .Where(x => x.SubAgentId == id)
            .Select(x => x.AgentId)
            .ToListAsync(cancellationToken);

        return await GetAgents(relations, cancellationToken);
    }

    public async Task<Guid> AddAgent(AgentInputModel inputModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        EnsureUserCanCreateEntities();

        EnsureAgentInputModelIsValid(inputModel);

        var entity = inputModel.MapToAgent();

        await _iopDbContext.Agents.AddAsync(entity, cancellationToken);
        await _iopDbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAgent(Guid id, AgentInputModel updateModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateModel, nameof(updateModel));

        var entity = await GetEnsuredEntity(id, asNoTracking: false, EntityIncludeLevel.All, cancellationToken);

        EnsureUserCanUpdateEntity(entity);

        EnsureAgentInputModelIsValid(updateModel, id);

        updateModel.MapToAgent(entity);

        _iopDbContext.SetMainEntityStateToModified(entity);
        await _iopDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAgent(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEnsuredEntity(id, cancellationToken: cancellationToken);

        EnsureUserCanDeleteEntity(entity);

        _iopDbContext.Remove(entity);
        await _iopDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid> GetAgentId(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier, nameof(identifier));

        var query = CreateGetAgentsQuery(asNoTracking: true, EntityIncludeLevel.Minimal, filter: x => x.Identifier == identifier);

        var agent = await query.SingleOrDefaultAsync(x => x.Identifier == identifier, cancellationToken)
            ?? throw new NotFoundException($"No agent with the identifier '{identifier}' has been found.");

        return agent.Id;
    }

    protected async override Task<Agent> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal, 
        CancellationToken cancellationToken = default)
    {
        var query = CreateGetAgentsQuery(asNoTracking, entityIncludeLevel);

        var entity = await query.SingleOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException("No resource has been found.");

        return entity;
    }

    private IQueryable<Agent> CreateGetAgentsQuery(
        bool asNoTracking,
        EntityIncludeLevel entityIncludeLevel,
        Expression<Func<Agent, bool>>? filter = null)
    {
        filter ??= x => true;

        var query = asNoTracking
            ? _iopDbContext.Agents.AsNoTracking()
            : _iopDbContext.Agents.AsQueryable();

        query = entityIncludeLevel switch
        {
            EntityIncludeLevel.All => buildEntityIncludeLevelAllQuery(query),
            _ => query.Include(d => d.Name),
        };

        static IQueryable<Agent> buildEntityIncludeLevelAllQuery(IQueryable<Agent> query)
        {
            query = query
                .Include(d => d.ContactPoint)
                .Include(d => d.Description)
                .Include(d => d.Images)
                .Include(d => d.Name)
                .Include(d => d.SubAgents)
                    .ThenInclude(s => s.SubAgent)
                ;

            return query;
        }

        return query
            .Where(filter)
            .OrderBy(x => x.Id);
    }

    private void EnsureAgentInputModelIsValid(AgentInputModel model, Guid? id = null)
    {
        var context = new ValidationContext<AgentInputModel>(model);
        var result = _agentValidator.Validate(context);

        var identifierExists = id.HasValue
            ? _iopDbContext.Agents.Any(x => x.Identifier == model.Identifier && x.Id != id)
            : _iopDbContext.Agents.Any(x => x.Identifier == model.Identifier);

        if (identifierExists)
        {
            result.Errors.Add(new(nameof(model.Identifier), "An agent with the same identifier already exists."));
        }

        foreach (var item in model.SubAgents)
        {
            if (_iopDbContext.Agents.Find(item.Id) is null)
            {
                result.Errors.Add(new(nameof(model.SubAgents), $"No agent with the id '{item.Id}' has been found."));
            }
        }

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
