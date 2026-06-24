using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record GetPublicationLevelInfoCommand(PublishableType Type, Guid Id) : IRequest<PublicationLevelInfoModel>
{ }
