namespace Bfs.Iop.Core.Abstractions.Models.LinkedData;

public sealed record SchemaGraph
{
    public required string SchemaName { get; set; }

    public IEnumerable<SchemaClass> Classes { get; init; } = [];
}
