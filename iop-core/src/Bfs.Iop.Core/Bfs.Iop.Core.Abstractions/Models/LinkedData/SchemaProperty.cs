using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Abstractions.Models.LinkedData;

public sealed record SchemaProperty
{
    public Uri Path { get; set; } = null!;

    public List<string> AllowedValues { get; set; } = [];

    public Uri? ConformsTo { get; set; }

    public MultiLanguageModel? Description { get; set; }

    public MultiLanguageModel? Label { get; set; }

    public string? DataType { get; set; }

    public string? Pattern { get; set; }

    public Uri? UriComplete { get; set; }

    public string? Unit { get; set; }

    public Uri? ToClassUri { get; set; }

    public string? Identifier { get; set; }

    public int? MinCardinality { get; set; }

    public int? MaxCardinality { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    public int? Order { get; set; }

}
