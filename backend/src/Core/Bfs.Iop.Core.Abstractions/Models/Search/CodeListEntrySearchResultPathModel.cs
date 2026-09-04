using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models.Search;

public record CodeListEntrySearchResultPathModel
{
    public string? ParentCode { get; init; }

    public required string Code { get; init; }

    public required MultiLanguageModel Name { get; init; }
}
