using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Response;

[XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public class QueryUsersResponseEnvelope
{
    [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public QueryUsersResponseBody? Body { get; set; }
}
