using System.Threading.Tasks;

namespace Bfs.Iop.Core.ApiClient;

public interface ITokenRetriever
{
    Task<string> GetAuthTokenAsync();
}