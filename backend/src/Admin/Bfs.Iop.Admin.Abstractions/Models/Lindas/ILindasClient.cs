using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Models.Lindas;

public interface ILindasClient
{
    Task<Uri?> GetRdfUrlAsync(
        LindasResourceType type,
        string identifier,
        string? version,
        CancellationToken cancellationToken);

    Task<Uri?> GetLdUriAsync(
        LindasResourceType type,
        string identifier,
        string? version,
        CancellationToken cancellationToken);
}
