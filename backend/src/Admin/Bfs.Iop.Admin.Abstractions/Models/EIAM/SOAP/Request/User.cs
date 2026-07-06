using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Request;

public class User
{
    [XmlElement(ElementName = "clientExtId")]
    public string? ClientExtId { get; set; }

    [XmlElement(ElementName = "firstName")]
    public string? FirstName { get; set; }

    [XmlElement(ElementName = "name")]
    public string? Name { get; set; }

    [XmlElement(ElementName = "email")]
    public string? Email { get; set; }
}
