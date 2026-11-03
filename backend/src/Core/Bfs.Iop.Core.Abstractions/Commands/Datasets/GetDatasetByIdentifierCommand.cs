using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetByIdentifierCommand(string Identifier) : IRequest<DcatDatasetModel>
{ }
