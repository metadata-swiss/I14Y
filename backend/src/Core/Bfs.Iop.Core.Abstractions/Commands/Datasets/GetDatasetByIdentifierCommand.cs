using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetByIdentifierCommand(string Identifier) : IRequest<DcatDatasetModel>
{ }
