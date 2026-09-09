using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

public class QueryUsersRequestBody
{
    [XmlElement(ElementName = "queryUsers", Namespace = "http://adnovum.ch/nevisidm/ws/services/v1")]
    public QueryUsersRequest? QueryUsers { get; set; }
}
