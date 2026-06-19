namespace Bfs.Iop.Core.Data.Entities;

internal sealed class VocabularyConfig : EntityBase
{
    public string VocabularyIdentifier { get; set; } = null!;

    public string ConceptIdentifier { get; set; } = null!;

    public string ConceptVersion { get; set;} = null!;
}
