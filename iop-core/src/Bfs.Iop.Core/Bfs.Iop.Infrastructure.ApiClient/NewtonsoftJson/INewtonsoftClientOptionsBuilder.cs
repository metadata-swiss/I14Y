using Newtonsoft.Json;

namespace Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

public interface INewtonsoftClientOptionsBuilder : IClientOptionsBuilder
{
    void AddUpdateJsonSerializerSettings(Action<JsonSerializerSettings> settingsUpdater);
}
