using Newtonsoft.Json;

namespace Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

internal class NewtonsoftClientOptionsBuilder : ClientOptionsBuilderBase, INewtonsoftClientOptionsBuilder
{
    public List<Action<JsonSerializerSettings>> SettingsUpdater { get; } = new();

    public void AddUpdateJsonSerializerSettings(Action<JsonSerializerSettings> settingsUpdater)
    {
        SettingsUpdater.Add(settingsUpdater);
    }
}
