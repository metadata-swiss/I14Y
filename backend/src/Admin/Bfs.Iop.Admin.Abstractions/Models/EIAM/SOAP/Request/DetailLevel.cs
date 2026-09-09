using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

public class DetailLevel
{
    [XmlElement(ElementName = "userDetailLevel")]
    public string? UserDetailLevel { get; set; }
}
