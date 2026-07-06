using System.Collections.Generic;
using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Response;

[XmlRoot(ElementName = "queryUsersResponse")]
public class QueryUsersResponse
{
    [XmlElement(ElementName = "return", Namespace = "")]
    public List<UserReturn>? UserReturn { get; set; }
}
