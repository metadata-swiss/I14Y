using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record GetPublicServiceRequiresCommand(Guid PublicServiceId) : IRequest<PagedResult<PublicServiceModel>>
{ }
