using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record GetPublicServicesCommand(
    string? PublicServiceIdentifier,
    string? PublisherIdentifier,
    string? TenantIdentifier,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<PublicServiceModel>>
{ }
