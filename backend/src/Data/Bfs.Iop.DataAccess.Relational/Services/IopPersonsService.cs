using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Services;

internal sealed class IopPersonsService : AuthorizedEntityServiceBase<IopPerson>, IIopPersonsService
{
    private readonly IopDbContext _dbContext;

    protected override IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDeleteEntity => 
        [BusinessRole.InteroperabilityService];

    public IopPersonsService(
        IopDbContext dbContext,
        IEntityAuthorizationService entityAuthorizationService, 
        IUserContextService userContextService) : base(entityAuthorizationService, userContextService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IopPersonModel> GetIopPerson(Guid id, CancellationToken cancellationToken)
    {
        var entity = await GetEnsuredEntity(id, asNoTracking: true, EntityIncludeLevel.All, cancellationToken);

        return entity.MapToIopPersonModel();
    }

    public async Task<IopPersonModel> GetIopPersonByEmail(string email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        var result = await _dbContext.IopPersons.SingleOrDefaultAsync(
                i => i.Email.ToLower() == email.ToLower(), cancellationToken) ??
                throw new NotFoundException($"No person with the email '{email}' has been found.");

        return result.MapToIopPersonModel();
    }

    public async Task<Guid> GetIopPersonIdByEmail(string email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        var id = (await _dbContext.IopPersons.SingleOrDefaultAsync(
                    i => i.Email.ToLower() == email.ToLower(), cancellationToken))?.Id
                    ?? throw new NotFoundException($"No person with the email '{email}' has been found.");

        return id;
    }

    public async Task<PagedResult<IopPersonModel>> SearchIopPersons(
        string? query, 
        int page, 
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var request = _dbContext.IopPersons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var inputStrings = query
                .Trim()
                .ToLower() //the needle and haystack are both lowered to lowercase in order to enable case insensitive search this might have some drawbacks vs EF.Functions.ILike but they should be neglible
                .Split(' ')
                .Take(3); //limit queries to decrease db strain

            request = request
                .Where(p => inputStrings.Any(substring =>
                    p.GivenName.ToLower().Contains(substring) ||
                    p.FamilyName.ToLower().Contains(substring) ||
                    p.Email.ToLower().Contains(substring)));
        }

        var totalResults = await request.CountAsync(cancellationToken);

        var results = await request
            .Skip((page -1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalResults : pageSize,
            Results = results.Select(x => x.MapToIopPersonModel()).ToList().AsReadOnly(),
            TotalCount = totalResults
        };
    }

    public async Task<Guid> AddIopPerson(IopPersonModel iopPersonModel, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(iopPersonModel, nameof(iopPersonModel));

        EnsureUserCanCreateEntities();

        var exists = await _dbContext.IopPersons.AnyAsync(p => p.Email.ToLower() == iopPersonModel.Email.ToLower(), cancellationToken);

        if (exists)
        {
            throw new ConflictException($"A person with the email '{iopPersonModel.Email}' already exists.");
        }

        var entity = iopPersonModel.MapToIopPerson();
        entity.FirstLoginDate = DateOnly.MinValue;
        entity.LastLoginDate = DateOnly.MinValue;

        _dbContext.IopPersons.Add(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task AddOrUpdateCurrentIopPerson(CancellationToken cancellationToken)
    {
        if (!_userContextService.UserHasRole(IopClaimsHelper.Roles.General.Allow))
        {
            throw new ForbiddenException("User doesn't have valid role claim.");
        }

        var userGivenName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType);
        var userFamilyName = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType);
        var userEmail = _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType);

        if (string.IsNullOrWhiteSpace(userGivenName) ||
            string.IsNullOrWhiteSpace(userFamilyName) ||
            string.IsNullOrWhiteSpace(userEmail))
        {
            //somethings missing in the token -> not a valid user
            throw new ForbiddenException("User is missing token information.");
        }

        var entity = await _dbContext.IopPersons
            .SingleOrDefaultAsync(p => p.Email.ToLower() == userEmail.ToLower(), cancellationToken);

        if (entity is null)
        {
            entity = new IopPerson()
            {
                Email = userEmail,
                FamilyName = userFamilyName,
                GivenName = userGivenName,
            };

            _dbContext.IopPersons.Add(entity);
        }
        else
        {
            entity.LastLoginDate = DateOnly.FromDateTime(DateTimeOffset.Now.DateTime);
            _dbContext.SetMainEntityStateToModified(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    protected override async Task<IopPerson> GetEnsuredEntity(
        Guid id, 
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking
            ? _dbContext.IopPersons.AsNoTracking()
            : _dbContext.IopPersons.AsQueryable();

        query = entityIncludeLevel switch
        {
            _ => query
        };

        var entity = await query.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException($"No resource has been found.")
            : entity;
    }
}
