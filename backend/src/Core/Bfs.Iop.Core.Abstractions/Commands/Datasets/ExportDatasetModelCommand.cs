using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record ExportDatasetModelCommand(Guid DatasetId, LinkedDataFormat Format) : IRequest<ExportFile>;