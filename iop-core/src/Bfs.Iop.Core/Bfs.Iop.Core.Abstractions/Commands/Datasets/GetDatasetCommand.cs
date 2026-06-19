using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetCommand(Guid DatasetId) : IRequest<DcatDatasetModel>
{ }
