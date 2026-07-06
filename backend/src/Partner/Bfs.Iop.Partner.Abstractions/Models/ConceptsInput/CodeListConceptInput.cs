using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Partner.Models.ConceptsInput;

public sealed class CodeListConceptInput : ConceptInputBase
{
    public CodeListEntrySortProperty? CodeListEntryDefaultSortProperty { get; init; }

    public required CodeListEntryValueType CodeListEntryValueType { get; init; }

    public required int CodeListEntryValueMaxLength { get; init; }
}
