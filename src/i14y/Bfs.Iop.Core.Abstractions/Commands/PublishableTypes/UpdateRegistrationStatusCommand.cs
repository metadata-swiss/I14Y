using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record UpdateRegistrationStatusCommand(PublishableType Type, Guid Id, RegistrationStatus Status) : IRequest
{ }
