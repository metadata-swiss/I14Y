using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeList;

internal static class CodeListDocumentFactory
{
    public static IndexRequest Build(CodeListIndexDocument entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var doc = new Dictionary<string, object?>
        {
            [EsCodeListFields.Id] = entry.Id.ToString(),
            [EsCodeListFields.ConceptId] = entry.ConceptId.ToString(),
            [EsCodeListFields.Code] = entry.Code,
        };

        SetIfPresent(doc, EsCodeListFields.ParentCode, entry.ParentCode);

        if (entry.AncestorCodes.Count > 0)
        {
            doc[EsCodeListFields.AncestorCodes] = entry.AncestorCodes;
        }

        SetMultiLang(doc, EsCodeListFields.Name, entry.Name);
        SetMultiLang(doc, EsCodeListFields.Description, entry.Description);

        var annotations = entry.Annotations.Select(BuildAnnotation).Where(x => x.Count > 0).ToArray();

        if (annotations.Length > 0)
        {
            doc[EsCodeListFields.Annotations] = annotations;
        }

        return new IndexRequest(entry.Id.ToString(), doc);
    }

    private static Dictionary<string, object?> BuildAnnotation(AnnotationInputModel annotation)
    {
        var doc = new Dictionary<string, object?>();


        SetIfPresent(doc, EsCodeListFields.Annotation.Type, annotation.Type);
        SetIfPresent(doc, EsCodeListFields.Annotation.Identifier, annotation.Identifier);
        SetIfPresent(doc, EsCodeListFields.Annotation.Title, annotation.Title);
        SetIfPresent(doc, EsCodeListFields.Annotation.Uri, annotation.Uri);

        SetMultiLang(doc, EsCodeListFields.Annotation.Text, annotation.Text);

        return doc;
    }

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            doc[field] = value;
        }
    }

    private static void SetMultiLang(Dictionary<string, object?> doc, string field, MultiLanguageModel? text)
    {
        if (text is null || text.IsContentNullOrWhiteSpace())
        {
            return;
        }

        doc[field] = text.ToDictionary().ToDictionary(x => x.Key, x => (object?)x.Value);
    }
}
