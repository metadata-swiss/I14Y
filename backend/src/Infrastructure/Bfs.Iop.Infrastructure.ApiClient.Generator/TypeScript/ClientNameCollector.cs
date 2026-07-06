namespace Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;

internal sealed class ClientNameCollector
{
    private readonly List<string> _clientNames = [];

    static ClientNameCollector() => 
        Instance = new ClientNameCollector();

    public static ClientNameCollector Instance { get; }

    public IEnumerable<string> Names => _clientNames.AsReadOnly();

    public void PushClientName(string clientName)
    {
        if (!string.IsNullOrWhiteSpace(clientName) && !_clientNames.Contains(clientName))
        {
            _clientNames.Add(clientName);
        }
    }
}
