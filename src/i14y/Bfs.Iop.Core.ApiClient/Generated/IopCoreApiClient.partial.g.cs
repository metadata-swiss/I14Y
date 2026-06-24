
namespace Bfs.Iop.Core.ApiClient;
internal partial class IopCoreApiClient
{
    partial void Initialize()
    {
        _instanceSettings = new Newtonsoft.Json.JsonSerializerSettings();
        base.UpdateInstanceJsonSerializerSettings(_instanceSettings);
    }
}
