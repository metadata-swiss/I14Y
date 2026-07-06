using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record GetDatasetsCommand(
    string? AccessRights,
    string? DatasetIdentifier,
    string? PublisherIdentifier,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<DcatDatasetModel>>
{ }
