namespace Bfs.Iop.Core.Data.Entities;

internal sealed class Resource : EntityBase
{
    public DataService? DataServiceConformsTo { get; set; }

    public Guid? DataServiceConformsToId { get; set; }

    public DataService? DataServiceDocumentation { get; set; }

    public Guid? DataServiceDocumentationId { get; set; }

    public DataService? DataServiceEndpointDescription { get; set; }

    public Guid? DataServiceEndpointDescriptionId { get; set; }

    public DataService? DataServiceEndpointUrl { get; set; }

    public Guid? DataServiceEndpointUrlId { get; set; }

    public DataService? DataServiceLandingPage { get; set; }

    public Guid? DataServiceLandingPageId { get; set; }

    public Dataset? DataSetConformsTo { get; set; }

    public Guid? DataSetConformsToId { get; set; }

    public Dataset? DatasetDocumentation { get; set; }

    public Guid? DatasetDocumentationId { get; set; }

    public Dataset? DataSetImage { get; set; } = null;

    public Guid? DataSetImageId { get; set; }

    public Dataset? DataSetIsReferencedBy { get; set; }

    public Guid? DataSetIsReferencedById { get; set; }

    public Dataset? DatasetLandingPage { get; set; }

    public Guid? DatasetLandingPageId { get; set; }

    public Dataset? DataSetRelation { get; set; }

    public Guid? DataSetRelationId { get; set; }

    public Distribution? DistributionAccessUrl { get; set; }

    public Guid? DistributionAccessUrlId { get; set; }

    public Distribution? DistributionConformsTo { get; set; }

    public Guid? DistributionConformsToId { get; set; }

    public Distribution? DistributionDocumentation { get; set; }

    public Guid? DistributionDocumentationId { get; set; }

    public Distribution? DistributionDownloadUrl { get; set; }

    public Guid? DistributionDownloadUrlId { get; set; }

    public Distribution? DistributionImage { get; set; }

    public Guid? DistributionImageId { get; set; }

    public IopConcept? IopConceptConformsTo { get; set; }

    public Guid? IopConceptConformsToId { get; set; }

    public IopConcept? IopConceptReplaces { get; set; }

    public Guid? IopConceptReplacesId { get; set; }

    public MappingTable? MappingTableConformsTo { get; set; }

    public Guid? MappingTableConformsToId { get; set; }

    public string Href { get; set; } = null!;

    public MultiLanguage? Label { get; set; }

    public QualifiedRelation? QualifiedRelation { get; set; }

    public Guid? QualifiedRelationId { get; set; }

    public Agent? AgentImage { get; set; }

    public Guid? AgentImageId { get; set; }
}