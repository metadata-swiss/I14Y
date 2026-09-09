using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdateRegistrationStatusProposalCommand(PublishableResourceType Type, Guid Id, RegistrationStatus? Proposal) : IRequest
{ }
