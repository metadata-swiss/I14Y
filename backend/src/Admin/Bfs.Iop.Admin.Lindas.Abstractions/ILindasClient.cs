using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Lindas.Abstractions;

public interface ILindasClient
{
    Task<Uri?> GetLinkAsync(
        LindasLinkType linkType,
        LindasResourceType resourceType,
        string identifier,
        string? version,
        CancellationToken cancellationToken);
}