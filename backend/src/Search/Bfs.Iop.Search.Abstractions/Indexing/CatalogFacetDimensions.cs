namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// The identifiers of the facet dimensions returned by a catalog search count.
/// <para>
/// These are <b>not</b> index field names — the Elasticsearch document fields live in
/// <c>EsCatalogFields</c>, and the two sets deliberately differ in casing and spelling. What these
/// strings key is the facet dictionary the count response carries.
/// </para>
/// <para>
/// <b>They are a UI contract.</b> admin-ui and public-ui look facets up by these exact names, so
/// renaming one empties that facet in the browser with HTTP 200 and nothing logged server-side. Add
/// a dimension in <c>CatalogQueryBuilder</c> (both the facet list and the filter clauses) and read it
/// in <c>CatalogSearchCountQueryService</c>; a name that appears in only one of those places is a
/// dimension that silently counts nothing.
/// </para>
/// </summary>
public static class CatalogFacetDimensions
{
    public const string AccessRights = "AccessRights";
    public const string BusinessEvents = "BusinessEvents";
    public const string ConceptType = "ConceptType";
    public const string Formats = "Formats";
    public const string HasStructure = "HasStructure";
    public const string LifeEvents = "LifeEvents";
    public const string PublicationLevel = "PublicationLevel";
    public const string PublicationLevelProposal = "PublicationLevelProposal";
    public const string PublisherIdentifier = "PublisherIdentifier";
    public const string RegistrationStatus = "RegistrationStatus";
    public const string RegistrationStatusProposal = "RegistrationStatusProposal";
    public const string Themes = "Themes";
    public const string Type = "Type";
}
