using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record UpdateDatasetModelPositionCommand(Guid DatasetId, Dictionary<string, SchemaPoint> ClassesPositionInput) : IRequest;