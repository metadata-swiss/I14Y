using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetModelGraphCommand(Guid DatasetId) : IRequest<SchemaGraph>;