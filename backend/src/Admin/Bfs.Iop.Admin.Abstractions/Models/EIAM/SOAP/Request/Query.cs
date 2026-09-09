using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

public class Query
{
    [XmlElement(ElementName = "detailLevels")]
    public DetailLevel? DetailLevel { get; set; }

    [XmlElement(ElementName = "user")]
    public User? User { get; set; }
}
