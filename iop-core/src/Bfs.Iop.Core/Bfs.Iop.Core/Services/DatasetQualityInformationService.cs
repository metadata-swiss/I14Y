using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.EntityFrameworkCore;
using Bfs.Iop.Core.Authorization.Contracts;

namespace Bfs.Iop.Core.Services;

internal sealed class DatasetQualityInformationService : AuthorizedEntityServiceBase<DatasetQualityInformation>, IDatasetQualityInformationService
{
    private readonly IDatasetsService _datasetsService;
    private readonly IopDbContext _dbContext;

    public DatasetQualityInformationService(
        IDatasetsService datasetsService,
        IopDbContext dbContext,
        IEntityAuthorizationService entityAuthorizationService,
        IUserContextService userContextService) : base(
            entityAuthorizationService,
            userContextService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PagedResult<DatasetQualityQuestionModel>> GetQualityQuestions(
        int page, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var totalCount = await _dbContext.DatasetQualityQuestions.CountAsync(cancellationToken);

        var results = await _dbContext.DatasetQualityQuestions
            .Include(d => d.AnswerOptions)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<DatasetQualityQuestionModel>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? totalCount : pageSize,
            Results = results.Select(x => x.MapToDatasetQualityQuestionModel()),
            TotalCount = totalCount,
        };
    }

    protected override void EnsureUserCanDeleteEntity(DatasetQualityInformation entity)
    {
        base.EnsureUserCanDeleteEntity(entity);

        var allowAction = _datasetsService.GetUserAllowActionInfo(entity.DatasetId, default);
    }

    protected override Task<DatasetQualityInformation> GetEnsuredEntity(
        Guid id, 
        bool asNoTracking = false,
        EntityIncludeLevel entityIncludeLevel = EntityIncludeLevel.Minimal, 
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
