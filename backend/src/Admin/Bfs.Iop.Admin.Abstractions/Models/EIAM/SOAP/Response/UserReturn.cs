using System;
using System.Xml.Serialization;

namespace Bfs.Iop.Admin.Models.EIAM.SOAP.Response;

[XmlRoot(ElementName = "return")]
public class UserReturn
{
    [XmlElement("loginId")]
    public string? LoginId { get; set; }

    [XmlElement("extId")]
    public string? ExtId { get; set; }

    [XmlElement("clientExtId")]
    public string? ClientExtId { get; set; }

    [XmlElement("clientName")]
    public string? ClientName { get; set; }

    [XmlElement("state")]
    public string? State { get; set; }

    [XmlElement("lastLogin")]
    public DateTime? LastLogin { get; set; }

    [XmlElement("firstName")]
    public string? FirstName { get; set; }

    [XmlElement("name")]
    public string? Name { get; set; }

    [XmlElement("email")]
    public string? Email { get; set; }

    [XmlElement("language")]
    public string? Language { get; set; }

    [XmlElement("templateCollection")]
    public string? TemplateCollection { get; set; }
}
