using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Models.OpenData;

public interface IOpenDataIndex
{
    Task<OpenDataSearchResult> Search(string? query, string culture, int? page, int? pageSize, CancellationToken cancellationToken);
}