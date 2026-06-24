using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Contracts;

public interface IIopPersonsService : IAuthorizedEntityService
{
    Task<Guid> AddIopPerson(IopPersonModel iopPersonModel, CancellationToken cancellationToken);

    Task AddOrUpdateCurrentIopPerson(CancellationToken cancellationToken);

    Task<IopPersonModel> GetIopPerson(Guid id, CancellationToken cancellationToken);

    Task<IopPersonModel> GetIopPersonByEmail(string email, CancellationToken cancellationToken);

    Task<Guid> GetIopPersonIdByEmail(string email, CancellationToken cancellationToken);

    Task<PagedResult<IopPersonModel>> SearchIopPersons(string query, int page, int pageSize, CancellationToken cancellationToken);
}