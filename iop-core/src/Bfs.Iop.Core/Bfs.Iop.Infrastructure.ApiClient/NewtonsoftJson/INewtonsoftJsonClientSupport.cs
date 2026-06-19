namespace Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

public interface INewtonsoftJsonClientSupport : IWebApiClientSupport
{
    void SetupApiClientOptions(INewtonsoftClientOptionsBuilder optionsBuilder);
}
