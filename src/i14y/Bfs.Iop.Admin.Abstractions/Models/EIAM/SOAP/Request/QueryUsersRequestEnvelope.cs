using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class QueryUsersRequestEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public QueryUsersRequestBody? Body { get; set; }
}
