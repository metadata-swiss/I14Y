using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublishableTypes;

public sealed record GetRegistrationStatusInfoCommand(PublishableType Type, Guid Id) : IRequest<RegistrationStatusInfoModel>
{ }
