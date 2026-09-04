using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Vocabularies;

public abstract record IdentifiedVocabularyBase
{
    protected IdentifiedVocabularyBase()
    { }

    public IReadOnlyList<VocabularyEntryModel> Entries { get; init; } = [];

    public abstract string Identifier { get; }
}
