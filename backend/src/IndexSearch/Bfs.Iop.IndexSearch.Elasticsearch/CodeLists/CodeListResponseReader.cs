using System.Text.Json;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

internal static class CodeListResponseReader
{
    public static PagedResult<CodeListSearchHit> ReadSearch(
        JsonElement response,
        int page,
        int pageSize,
        out int skipped)
    {
        var hits = response.GetProperty("hits");

        var read = hits.GetProperty("hits").EnumerateArray()
            .Select(x => TryReadHit(x.GetProperty("_source")))
            .ToArray();

        skipped = read.Count(x => x is null);

        return new PagedResult<CodeListSearchHit>
        {
            Page = page,
            PageSize = pageSize,

            // Elasticsearch's count of what matched. A document we could not read still matched, so
            // this may exceed Results.Count rather than being quietly adjusted down to it.
            TotalCount = hits.GetProperty("total").GetProperty("value").GetInt32(),
            Results = [.. read.OfType<CodeListSearchHit>()],
        };
    }

    // Both ids are required on the hit, and an entry carrying neither can be linked to nor filtered
    // by. Dropping that one row leaves the rest of the page answerable; throwing lost all of it.
    private static CodeListSearchHit? TryReadHit(JsonElement source)
    {
        if (!Guid.TryParse(ReadString(source, EsCodeListFields.Id), out var id)
            || !Guid.TryParse(ReadString(source, EsCodeListFields.ConceptId), out var conceptId))
        {
            return null;
        }

        return new CodeListSearchHit
        {
            Id = id,
            ConceptId = conceptId,
            Code = ReadString(source, EsCodeListFields.Code) ?? string.Empty,
            ParentCode = ReadString(source, EsCodeListFields.ParentCode),
            AncestorCodes = ReadStrings(source, EsCodeListFields.AncestorCodes),
            Name = ReadMultiLanguage(source, EsCodeListFields.Name),
            Description = ReadMultiLanguage(source, EsCodeListFields.Description),
            Annotations = ReadAnnotations(source),
        };
    }

    private static IReadOnlyList<AnnotationInputModel> ReadAnnotations(JsonElement source)
    {
        if (!source.TryGetProperty(EsCodeListFields.Annotations, out var annotations)
            || annotations.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return
        [
            .. annotations.EnumerateArray().Select(x => new AnnotationInputModel
            {
                Type = ReadString(x, EsCodeListFields.Annotation.Type) ?? string.Empty,
                Identifier = ReadString(x, EsCodeListFields.Annotation.Identifier),
                Title = ReadString(x, EsCodeListFields.Annotation.Title),
                Uri = ReadString(x, EsCodeListFields.Annotation.Uri),
                Text = ReadMultiLanguage(x, EsCodeListFields.Annotation.Text),
            }),
        ];
    }

    private static IReadOnlyList<string> ReadStrings(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Array
            ? [.. value.EnumerateArray().Select(x => x.GetString()).OfType<string>()]
            : [];

    private static string? ReadString(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static MultiLanguageModel? ReadMultiLanguage(JsonElement source, string field)
    {
        if (!source.TryGetProperty(field, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var text = new MultiLanguageModel
        {
            De = ReadString(value, MultiLanguageModel.GermanKey),
            En = ReadString(value, MultiLanguageModel.EnglishKey),
            Fr = ReadString(value, MultiLanguageModel.FrenchKey),
            It = ReadString(value, MultiLanguageModel.ItalianKey),
            Rm = ReadString(value, MultiLanguageModel.RomanshKey),
        };

        return text.IsContentNullOrWhiteSpace() ? null : text;
    }
}
