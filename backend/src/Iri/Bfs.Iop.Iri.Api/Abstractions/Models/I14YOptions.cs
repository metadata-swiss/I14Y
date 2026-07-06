using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Iri.Api.Abstractions.Models;

public sealed record I14YOptions
{
    public required string IopCoreApiUrl { get; init; }
    public required string PublicUiUrl { get; init; }

    public required Language DefaultLanguage { get; init; }
}
