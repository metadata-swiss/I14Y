using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Vocabularies;

public abstract record IdentifiedVocabularyBase
{
    protected IdentifiedVocabularyBase()
    { }

    public IReadOnlyList<VocabularyEntryModel> Entries { get; init; } = [];

    public abstract string Identifier { get; }
}
