namespace Bfs.Iop.Core.LinkedData.Configuration;
internal class TripleStoreConfiguration
{
    public string QueryEndpoint { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
}
