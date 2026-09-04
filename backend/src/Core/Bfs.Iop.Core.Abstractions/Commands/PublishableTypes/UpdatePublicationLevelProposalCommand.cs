using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdatePublicationLevelProposalCommand(PublishableResourceType Type, Guid Id, PublicationLevel? Proposal) : IRequest
{ }
