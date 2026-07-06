using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdatePublicationLevelCommand(PublishableType Type, Guid Id, PublicationLevel Level) : IRequest
{ }
