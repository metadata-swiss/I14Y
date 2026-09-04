namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class VocabularyConfig : EntityBase
{
    public string VocabularyIdentifier { get; set; } = null!;

    public string ConceptIdentifier { get; set; } = null!;

    public string ConceptVersion { get; set;} = null!;
}
