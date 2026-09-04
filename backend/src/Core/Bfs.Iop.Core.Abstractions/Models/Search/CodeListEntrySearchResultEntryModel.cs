using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models.Search;

public record CodeListEntrySearchResultEntryModel : SearchResultEntryModel<CodeListEntryModel>
{
    public IEnumerable<CodeListEntrySearchResultPathModel> Path { get; init; } = [];
}
