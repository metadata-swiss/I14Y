using System.Net.Http.Headers;
using System.Text;
using Bfs.Iop.Core.LinkedData.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace Bfs.Iop.Core.LinkedData.Factories;

internal class FusekiConnectionFactory
{
    private readonly TripleStoreConfiguration _tripleStoreConfiguration;

    public FusekiConnectionFactory(TripleStoreConfiguration tripleStoreConfiguration)
    {
        ArgumentNullException.ThrowIfNull(tripleStoreConfiguration);
        _tripleStoreConfiguration = tripleStoreConfiguration;
    }

    public FusekiConnector CreateFusekiConnector()
    {
        var connector = new FusekiConnector(_tripleStoreConfiguration.Endpoint);
        connector.SetCredentials(
            _tripleStoreConfiguration.Username,
            _tripleStoreConfiguration.Password);

        return connector;
    }

    public SparqlQueryClient CreateQueryClient()
    {
        var httpClient = new HttpClient();

        var token = Convert.ToBase64String(
            Encoding.ASCII.GetBytes(
                $"{_tripleStoreConfiguration.Username}:{_tripleStoreConfiguration.Password}"));

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", token);

        var client = new SparqlQueryClient(
            httpClient,
            new Uri(_tripleStoreConfiguration.QueryEndpoint));

        return client;
    }
}