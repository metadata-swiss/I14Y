using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record UpdateDatasetCommand(Guid DatasetId, DcatDatasetInputModel DatasetInput) : IRequest
{ }
