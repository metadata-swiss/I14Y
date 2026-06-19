using Newtonsoft.Json;
using System.Text;

namespace Bfs.Iop.Infrastructure.ApiClient.NewtonsoftJson;

public class NewtonsoftJsonWebApiClientBase : WebApiClientBase
{
    /// <inheritdoc cref="WebApiClientBase.RequestStringUpdater"/>
    protected override List<Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task>> RequestStringUpdater { get; }

    /// <inheritdoc cref="WebApiClientBase.RequestStringBuilderUpdater"/>
    protected override List<Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task>> RequestStringBuilderUpdater { get; }

    /// <inheritdoc cref="WebApiClientBase.ResponseProcessor"/>
    protected override List<Func<HttpClient, HttpResponseMessage, CancellationToken, Task>> ResponseProcessor { get; }

    /// <inheritdoc cref="WebApiClientBase.HttpClientFactory"/>
    protected override Func<Task<HttpClient>>? HttpClientFactory { get; }

    private List<Action<JsonSerializerSettings>> SettingsUpdater { get; }

    /// <inheritdoc cref="NewtonsoftJsonWebApiClientBase"/>
    public NewtonsoftJsonWebApiClientBase(INewtonsoftJsonClientSupport clientSupport)
    {
        var optionsBuilder = new NewtonsoftClientOptionsBuilder();

        clientSupport.SetupApiClientOptions(optionsBuilder);

        RequestStringUpdater = optionsBuilder.RequestStringUpdater;
        RequestStringBuilderUpdater = optionsBuilder.RequestStringBuilderUpdater;
        ResponseProcessor = optionsBuilder.ResponseProcessor;
        HttpClientFactory = optionsBuilder.HttpClientFactory;
        SettingsUpdater = optionsBuilder.SettingsUpdater;
    }

    /// <summary>
    /// Intercepts the Json serializer settings configuration
    /// </summary>
    /// <param name="settings">Json serializer settings to be updated</param>
    protected void UpdateInstanceJsonSerializerSettings(JsonSerializerSettings settings)
    {
        foreach (var settingsUpdater in SettingsUpdater)
        {
            settingsUpdater.Invoke(settings);
        }
    }
}
