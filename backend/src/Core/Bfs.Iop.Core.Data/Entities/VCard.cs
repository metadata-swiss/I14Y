using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Entities;

internal class VCard : EntityBase
{
    public Guid? AgentContactPointId { get; set; }

    public Agent? AgentContactPoint { get; set; }

    public MultiLanguage? AdrWork { get; set; }

    public VCardKind Kind { get; set; }

    public DataService? DataServiceContactPoint { get; set; }

    public Guid? DataServiceContactPointId { get; set; }

    public Dataset? DatasetContactPoint { get; set; }

    public Guid? DatasetContactPointId { get; set; }

    public string? EmailInternet { get; set; }

    public MultiLanguage? Fn { get; set; }

    public MultiLanguage? Note { get; set; }

    public MultiLanguage? Org { get; set; }

    public string? TelWorkVoice { get; set; }
}