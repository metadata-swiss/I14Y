using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdatePublicationLevelCommand(PublishableResourceType Type, Guid Id, PublicationLevel Level) : IRequest
{ }
