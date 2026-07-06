using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Models.Geocat;

public interface IGeocatIndex
{
    Task<GeocatSearchResult> Search(string? query, string culture, int? page, int? pageSize, CancellationToken cancellationToken);
}