using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Catalog;

public sealed record GetRelationsCountCommand(
    IReadOnlyList<RelationsCountRequestItem> Items) : IRequest<IReadOnlyList<RelationsCountModel>>
{ }
