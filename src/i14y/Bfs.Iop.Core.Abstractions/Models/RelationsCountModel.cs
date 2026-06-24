namespace Bfs.Iop.Core.Abstractions.Models;

/// <summary>
/// The number of incoming references ("relations-by" count) to a single catalogue resource,
/// together with a per-type breakdown. Sub-counts are <c>null</c> when they do not apply to
/// the resource type.
/// </summary>
public sealed record RelationsCountModel
{
    public required Guid Id { get; init; }

    /// <summary>Sum of all populated sub-counts.</summary>
    public int Total { get; init; }

    /// <summary>Concepts: structure attributes whose IRI matches the concept IRI.</summary>
    public int? StructureAttribute { get; init; }

    /// <summary>Concepts: mapping tables referencing the concept IRI (counted once per table).</summary>
    public int? MappingTable { get; init; }

    /// <summary>Datasets: data services serving the dataset.</summary>
    public int? DataService { get; init; }

    /// <summary>Datasets / public services: public services referencing the resource.</summary>
    public int? PublicService { get; init; }

    /// <summary>Data services: datasets referencing the data service via their distributions.</summary>
    public int? Dataset { get; init; }
}
