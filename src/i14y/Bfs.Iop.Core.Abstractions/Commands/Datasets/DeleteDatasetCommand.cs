using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record DeleteDatasetCommand(Guid DatasetId) : IRequest 
{ }
