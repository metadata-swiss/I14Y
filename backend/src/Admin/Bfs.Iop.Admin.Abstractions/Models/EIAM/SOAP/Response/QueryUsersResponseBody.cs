using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Response;

[XmlRoot(ElementName = "Body")]
public class QueryUsersResponseBody
{
    [XmlElement(ElementName = "queryUsersResponse", Namespace = "http://adnovum.ch/nevisidm/ws/services/v1")]
    public QueryUsersResponse? QueryUsersResponse { get; set; }
}
