
namespace Bfs.Iop.Admin.Api.ApiClient;
internal partial class IopAdminApiClient
{
    partial void Initialize()
    {
        _instanceSettings = new Newtonsoft.Json.JsonSerializerSettings();
        base.UpdateInstanceJsonSerializerSettings(_instanceSettings);
    }
}
