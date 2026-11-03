using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.MappingTables;

public sealed record GetMappingTablesCommand(
    string? MappingTableIdentifier,
    string? PublisherIdentifier,
    string? Version,
    string? CodeSystemUri,
    PublicationLevel? PublicationLevel,
    RegistrationStatus? RegistrationStatus,
    int? Page,
    int? PageSize) : IRequest<PagedResult<MappingTableModel>>
{ }
