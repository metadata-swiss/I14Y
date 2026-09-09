using System.Text.Json;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.IndexSearch.Elasticsearch.CodeList;


internal static class CodeListResponseReader
{
    public static PagedResult<CodeListSearchHit> ReadSearch(JsonElement response, int page, int pageSize)
    {
        var hits = response.GetProperty("hits");

        return new PagedResult<CodeListSearchHit>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = hits.GetProperty("total").GetProperty("value").GetInt32(),
            Results = [.. hits.GetProperty("hits").EnumerateArray().Select(x => ReadHit(x.GetProperty("_source")))],
        };
    }

    private static CodeListSearchHit ReadHit(JsonElement source) => new()
    {
        Id = Guid.Parse(String(source, EsCodeListFields.Id)!),
        ConceptId = Guid.Parse(String(source, EsCodeListFields.ConceptId)!),
        Code = String(source, EsCodeListFields.Code) ?? string.Empty,
        ParentCode = String(source, EsCodeListFields.ParentCode),
        AncestorCodes = Strings(source, EsCodeListFields.AncestorCodes),
        Name = MultiLang(source, EsCodeListFields.Name),
        Description = MultiLang(source, EsCodeListFields.Description),
        Annotations = ReadAnnotations(source),
    };

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

                Type = String(x, EsCodeListFields.Annotation.Type) ?? string.Empty,
                Identifier = String(x, EsCodeListFields.Annotation.Identifier),
                Title = String(x, EsCodeListFields.Annotation.Title),
                Uri = String(x, EsCodeListFields.Annotation.Uri),
                Text = MultiLang(x, EsCodeListFields.Annotation.Text),
            }),
        ];
    }

    private static IReadOnlyList<string> Strings(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Array
            ? [.. value.EnumerateArray().Select(x => x.GetString()).OfType<string>()]
            : [];

    private static string? String(JsonElement source, string field) =>
        source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static MultiLanguageModel? MultiLang(JsonElement source, string field)
    {
        if (!source.TryGetProperty(field, out var value) || value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var text = new MultiLanguageModel
        {
            De = String(value, MultiLanguageModel.GermanKey),
            En = String(value, MultiLanguageModel.EnglishKey),
            Fr = String(value, MultiLanguageModel.FrenchKey),
            It = String(value, MultiLanguageModel.ItalianKey),
            Rm = String(value, MultiLanguageModel.RomanshKey),
        };

        return text.IsContentNullOrWhiteSpace() ? null : text;
    }
}
