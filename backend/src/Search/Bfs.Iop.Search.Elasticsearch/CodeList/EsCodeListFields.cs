namespace Bfs.Iop.Search.Elasticsearch.CodeList;

/// <summary>
/// Field names for the Elasticsearch codelist-entry index. Annotations are modelled as a single
/// <c>nested</c> array on the entry document, which is what keeps each annotation's field and value
/// bound together at query time.
/// </summary>
internal static class EsCodeListFields
{
    // Code-list search covers 4 languages — no Romansh, unlike the catalog index. Carried over from
    // the previous engine deliberately: adding "rm" here needs a reindex, not just a constant.
    public static readonly string[] Languages = ["de", "en", "fr", "it"];

    public const string Id = "id";
    public const string ConceptId = "conceptId";
    public const string Code = "code";
    public const string ParentCode = "parentCode";
    public const string Name = "name";               // multilingual object
    public const string Description = "description";  // multilingual object

    public const string Annotations = "annotations";  // nested array

    // Sub-fields inside each nested annotation.
    public const string AnnotationType = "type";
    public const string AnnotationIdentifier = "identifier";
    public const string AnnotationTitle = "title";
    public const string AnnotationUri = "uri";
    public const string AnnotationText = "text";      // multilingual object

    public static string Nested(string sub) => $"{Annotations}.{sub}";
}
