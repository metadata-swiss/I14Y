using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Contracts;

/// <summary>
/// Defines services where users may create, update and/or delete their managed entities.
/// </summary>
public interface IAuthorizedEntityService
{
    Task<IEnumerable<AllowActionResult>> GetUserAllowActionInfo(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AllowActionResult> GetUserAllowCreateInfo(
        CancellationToken cancellationToken = default);
}