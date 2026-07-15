using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Services;

// This service is still needed for the allow action.
// Just checking the DcatCatalog should be enough.

internal sealed class DcatCatalogRecordsService : AuthorizedEntityServiceBase<DcatCatalogRecord>, IDcatCatalogRecordsService
{
    private readonly IopDbContext _dbContext;
    private readonly IVocabulariesService _vocabulariesService;

    public DcatCatalogRecordsService(
        IVocabulariesService vocabulariesService,
        IopDbContext dbContext,
        IEntityAuthorizationService entityAuthorizationService,
        IUserContextService userContextService) : base(entityAuthorizationService, userContextService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _vocabulariesService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }

    protected override async Task<DcatCatalogRecord> GetEnsuredEntity(
        Guid id,
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking
            ? _dbContext.DcatCatalogRecords.AsNoTracking()
            : _dbContext.DcatCatalogRecords.AsQueryable();

        query = query
            .Include(d => d.PrimaryTopic)
            .Include(d => d.Themes);

        var entity = await query.SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        return entity is null
            ? throw new NotFoundException($"No resource has been found.")
            : entity;
    }
}
