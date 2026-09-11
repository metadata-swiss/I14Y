using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IopConceptStructureReferenceModel(string DatasetUri, string AttributeUri)
{
    public DatasetReferenceModel? Dataset { get; init; }
}
