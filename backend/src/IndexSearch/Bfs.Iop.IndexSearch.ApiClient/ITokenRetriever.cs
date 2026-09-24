namespace Bfs.Iop.IndexSearch.ApiClient;

public interface ITokenRetriever
{
    Task<string> GetAuthTokenAsync();
}
