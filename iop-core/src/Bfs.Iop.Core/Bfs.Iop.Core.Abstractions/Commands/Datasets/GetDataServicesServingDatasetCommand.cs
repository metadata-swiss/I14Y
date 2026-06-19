using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDataServicesServingDatasetCommand(
    Guid DatasetId,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DataServiceModel>>
{ }
