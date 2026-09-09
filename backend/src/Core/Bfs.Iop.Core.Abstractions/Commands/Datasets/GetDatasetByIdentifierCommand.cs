using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetByIdentifierCommand(string Identifier) : IRequest<DcatDatasetModel>
{ }
