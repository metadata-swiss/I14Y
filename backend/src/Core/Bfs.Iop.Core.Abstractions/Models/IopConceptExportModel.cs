using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IopConceptExportModel : IopConceptModel
{
    public IReadOnlyCollection<CodeListEntryModel>? CodeListEntries { get; set; }
}
