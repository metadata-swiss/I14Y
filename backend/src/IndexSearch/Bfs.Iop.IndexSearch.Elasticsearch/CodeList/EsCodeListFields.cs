using Bfs.Iop.IndexSearch.Contracts;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeList;

internal static class EsCodeListFields
{
    public static readonly IReadOnlyList<string> Languages = IndexLanguages.All;

    public const string Id = "id";
    public const string ConceptId = "conceptId";
    public const string Code = "code";
    public const string ParentCode = "parentCode";
    public const string AncestorCodes = "ancestorCodes";
    public const string Name = "name";
    public const string Description = "description";
    public const string Annotations = "annotations";

    public static readonly IReadOnlyList<string> NgramFields = [Name];

    public static readonly IReadOnlyList<string> PlainMultiLanguageFields = [Description];

    public static readonly IReadOnlyList<string> SearchableKeywordFields = [Code];

    internal static class Annotation
    {
        public const string Type = "type";
        public const string Identifier = "identifier";
        public const string Title = "title";
        public const string Uri = "uri";
        public const string Text = "text";

        public static readonly IReadOnlyList<string> SearchableKeywordFields = [Type, Identifier, Title, Uri];
    }
}
