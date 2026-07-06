using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DataServices;

public sealed record GetDataServicesCommand(
    string? AccessRights,
    string? DataServiceIdentifier,
    string? PublisherIdentifier,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DataServiceModel>>
{ }
