using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record CheckDatasetModelExistCommand(Guid DatasetId) : IRequest<bool>;