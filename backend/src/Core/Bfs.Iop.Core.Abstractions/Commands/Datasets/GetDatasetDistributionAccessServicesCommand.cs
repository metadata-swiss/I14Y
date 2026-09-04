using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetDistributionAccessServicesCommand(
    Guid DatasetId,
    Guid DistributionId,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DataServiceModel>>
{ }
