using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;
public sealed record CreateDatasetModelPropertyCommand(Guid DatasetId, SchemaProperty PropertyInput, Uri ClassUri) : IRequest<Uri>;
