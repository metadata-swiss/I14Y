using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record GetPublicServicesCommand(
    string? PublicServiceIdentifier,
    string? PublisherIdentifier,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<PublicServiceModel>>
{ }
