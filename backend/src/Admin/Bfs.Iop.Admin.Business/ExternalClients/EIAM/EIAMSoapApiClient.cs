using Bfs.Iop.Admin.Models.EIAM.SOAP.Request;
using Bfs.Iop.Admin.Models.EIAM.SOAP.Response;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Business.ExternalClients.EIAM;

internal class EIAMSoapApiClient : IEIAMSoapApiClient
{
    public const string ClientName = "EIAM";

    private readonly IHttpClientFactory _httpClientFactory;

    public EIAMSoapApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IList<UserReturn>> SearchUsersByQuery(string query)
    {
        var eIAMclient = _httpClientFactory.CreateClient(ClientName);

        var searchByName = SearchUsersByPropertyName(eIAMclient, query, nameof(User.Name));
        var searchByFirstName = SearchUsersByPropertyName(eIAMclient, query, nameof(User.FirstName));
        var searchByEmail = SearchUsersByPropertyName(eIAMclient, query, nameof(User.Email));

        var searchResults = await Task.WhenAll(searchByName, searchByFirstName, searchByEmail);

        var searchResultsCombined = searchResults[0].Concat(searchResults[1]).Concat(searchResults[2]);

        var searchResulstWithValidEmail = searchResultsCombined.Where(i => 
            !string.IsNullOrWhiteSpace(i.Email) && 
            i.Email.Contains('@', System.StringComparison.InvariantCultureIgnoreCase));

        var searchResultsGroupedAndSorted = searchResulstWithValidEmail
            .GroupBy(x => x.ExtId)
            .Select(group => new { 
                ExtId = group.Key,
                Count = group.Count(),
                Results = group
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var searchResultsDistinct = searchResultsGroupedAndSorted
            .Select(group => group.Results.First())
            .ToList();

        return searchResultsDistinct;
    }

    private static async Task<IList<UserReturn>> SearchUsersByPropertyName(
        HttpClient eIAMclient,
        string query,
        string propertyName)
    {
        var content = new StringContent(
            PrepareQueryUsersXmlRequest(query, propertyName),
            Encoding.UTF8,
            "text/xml");

        var response = await eIAMclient.PostAsync("", content);

        var responseXml = await response.Content.ReadAsStringAsync();

        var deserializer = new XmlSerializer(typeof(QueryUsersResponseEnvelope));

        using var reader = new StringReader(responseXml);

        var envelope = (QueryUsersResponseEnvelope?)deserializer.Deserialize(reader);

        return envelope?.Body?.QueryUsersResponse?.UserReturn ?? new List<UserReturn>();
    }

    private static string PrepareQueryUsersXmlRequest(string query, string propertyName)
    {
        using var stringWriter = new StringWriter();
        using var xmlWriter = XmlWriter.Create(
            stringWriter,
            GetXmlWriterSettings());

        var serializer = new XmlSerializer(typeof(QueryUsersRequestEnvelope));
        serializer.Serialize(
            xmlWriter,
            GetQueryUsersEnvelope(query, propertyName),
            GetXmlSerializerNamespaces());

        return stringWriter.ToString();
    }

    private static XmlSerializerNamespaces GetXmlSerializerNamespaces()
    {
        return new XmlSerializerNamespaces(new XmlQualifiedName[]
        {
            new XmlQualifiedName("v1", "http://adnovum.ch/nevisidm/ws/services/v1"),
            new XmlQualifiedName("soapenv", "http://schemas.xmlsoap.org/soap/envelope/")
        });
    }

    private static XmlWriterSettings GetXmlWriterSettings()
    {
        return new XmlWriterSettings()
        {
            OmitXmlDeclaration = true,
            Encoding = Encoding.UTF8,
            Indent = true
        };
    }

    private static QueryUsersRequestEnvelope GetQueryUsersEnvelope(string query, string propertyName)
    {
        const string userDetailLevel = "HIGH";

        return new QueryUsersRequestEnvelope()
        {
            Body = new QueryUsersRequestBody()
            {
                QueryUsers = new QueryUsersRequest()
                {
                    Query = new Query()
                    {
                        DetailLevel = new DetailLevel()
                        {
                            UserDetailLevel = userDetailLevel
                        },
                        User = new User()
                        {
                            Name = propertyName == nameof(User.Name) ?  $"{query}*" : null,
                            FirstName = propertyName == nameof(User.FirstName) ? $"{query}*" : null,
                            Email = propertyName == nameof(User.Email) ? $"*{query}*" : null,
                        }
                    }
                }
            }
        };
    }
}
