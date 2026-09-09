namespace Bfs.Iop.Partner.Models.ConceptsInput;

public sealed class StringConceptInput : ConceptInputBase
{
    public required int MaxLength { get; init; }

    public required int MinLength { get; init; }

    public string? Pattern { get; init; }
}
