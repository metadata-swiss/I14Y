using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdateRegistrationStatusProposalCommand(PublishableType Type, Guid Id, RegistrationStatus? Proposal) : IRequest
{ }
