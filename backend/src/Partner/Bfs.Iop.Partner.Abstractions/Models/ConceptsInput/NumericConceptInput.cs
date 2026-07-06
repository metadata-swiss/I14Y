namespace Bfs.Iop.Partner.Models.ConceptsInput;

public sealed class NumericConceptInput : ConceptInputBase
{
    public required decimal MaxValue { get; init; }

    public string? MeasurementUnit { get; init; }

    public required decimal MinValue { get; init; }

    public required int NumberDecimals { get; init; }

    public string? Pattern { get; init; }
}
