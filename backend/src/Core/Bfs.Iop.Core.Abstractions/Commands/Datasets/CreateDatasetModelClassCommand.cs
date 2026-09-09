using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;
public sealed record CreateDatasetModelClassCommand(Guid DatasetId, SchemaClass SchemaClassInput) : IRequest<Uri>;
