using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetCommand(Guid DatasetId) : IRequest<DcatDatasetModel>
{ }
