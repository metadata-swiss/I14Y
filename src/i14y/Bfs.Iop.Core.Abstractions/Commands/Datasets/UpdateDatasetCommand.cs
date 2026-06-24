using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record UpdateDatasetCommand(Guid DatasetId, DcatDatasetInputModel DatasetInput) : IRequest
{ }
