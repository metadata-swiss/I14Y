using System.ServiceModel;
using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

public class QueryUsersRequest
{
    [XmlElement(ElementName = "query", Namespace = "")]
    public Query? Query { get; set; }
}
