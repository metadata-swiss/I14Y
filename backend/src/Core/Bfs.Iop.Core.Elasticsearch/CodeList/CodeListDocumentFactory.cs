using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Elasticsearch.CodeList;

/// <summary>
/// Builds the JSON document for a code-list entry: the entry fields plus its annotations as a
/// <c>nested</c> array. Mirrors the content of the Lucene <c>CodeListEntryIndexService.BuildDocument</c>.
/// </summary>
internal static class CodeListDocumentFactory
{
    public static (string Id, Dictionary<string, object?> Document) Build(CodeListEntryModel entry)
    {
        var doc = new Dictionary<string, object?>
        {
            [EsCodeListFields.Id] = entry.Id.ToString(),
            [EsCodeListFields.ConceptId] = entry.ConceptId.ToString(),
            [EsCodeListFields.Code] = entry.Code,
        };

        if (!string.IsNullOrWhiteSpace(entry.ParentCode))
        {
            doc[EsCodeListFields.ParentCode] = entry.ParentCode;
        }

        SetMultiLang(doc, EsCodeListFields.Name, entry.Name);
        SetMultiLang(doc, EsCodeListFields.Description, entry.Description);

        var annotations = (entry.Annotations ?? [])
            .Select(BuildAnnotation)
            .ToArray();

        if (annotations.Length > 0)
        {
            doc[EsCodeListFields.Annotations] = annotations;
        }

        return (entry.Id.ToString(), doc);
    }

    // Annotation values are stored lowercased so the `.raw` keyword sub-fields support exact
    // (case-insensitive) filter matches, mirroring the Lucene `_raw` fields. The result models are
    // hydrated from the DB, so lowercasing the indexed copy has no effect on what users see.
    private static Dictionary<string, object?> BuildAnnotation(AnnotationModel annotation)
    {
        var a = new Dictionary<string, object?>
        {
            [EsCodeListFields.AnnotationType] = annotation.Type.ToLowerInvariant(),
        };

        if (!string.IsNullOrWhiteSpace(annotation.Identifier))
        {
            a[EsCodeListFields.AnnotationIdentifier] = annotation.Identifier.ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(annotation.Title))
        {
            a[EsCodeListFields.AnnotationTitle] = annotation.Title.ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(annotation.Uri))
        {
            a[EsCodeListFields.AnnotationUri] = annotation.Uri.ToLowerInvariant();
        }

        if (annotation.Text is not null)
        {
            var byLang = annotation.Text.ToDictionary();
            if (byLang.Count > 0)
            {
                a[EsCodeListFields.AnnotationText] = byLang.ToDictionary(x => x.Key, x => (object?)x.Value.ToLowerInvariant());
            }
        }

        return a;
    }

    private static void SetMultiLang(Dictionary<string, object?> doc, string field, MultiLanguageModel? model)
    {
        if (model is null)
        {
            return;
        }

        var byLang = model.ToDictionary();
        if (byLang.Count > 0)
        {
            doc[field] = byLang.ToDictionary(x => x.Key, x => (object?)x.Value);
        }
    }
}
