using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record GetDataServicesCommand(
    string? AccessRights,
    string? DataServiceIdentifier,
    string? PublisherIdentifier,
    string? TenantIdentifier,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DataServiceModel>>
{ }
