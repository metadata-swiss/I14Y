using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Vocabularies;

internal abstract record IdentifiedVocabularyBase
{
    protected IdentifiedVocabularyBase()
    { }

    public IReadOnlyList<VocabularyEntryModel> Entries { get; init; } = [];

    public abstract string Identifier { get; }
}
