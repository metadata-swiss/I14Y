using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record DeleteDatasetModelPropertyCommand(Guid DatasetId, Uri PropertyUri) : IRequest;
