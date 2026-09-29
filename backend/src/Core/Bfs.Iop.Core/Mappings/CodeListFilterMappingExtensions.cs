using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Client = Bfs.Iop.IndexSearch.ApiClient;

namespace Bfs.Iop.Core.Mappings;

internal static class CodeListFilterMappingExtensions
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static Client.CodeListSearchFilter MapToIndexSearchFilter(
        this IEnumerable<string> filters,
        FilterConfigurationModel configuration)
    {
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentNullException.ThrowIfNull(configuration);

        var all = new List<Client.CodeListAnnotationCriterion>();
        var any = new List<Client.CodeListAnnotationCriterion>();

        foreach (var selection in Parse(filters))
        {
            var definition = configuration.Filters.SingleOrDefault(x => x.FilterIdentifier.Equals(
                    selection.FilterIdentifier,
                    StringComparison.InvariantCultureIgnoreCase))
                ?? throw new InvalidOperationException(
                    $"FilterInputModel filter with identifier {selection.FilterIdentifier} does not exist.");

            var chosen = definition.Values.Where(x => selection.Values
                .Select(v => v.ToLowerInvariant())
                .Contains(x.FilterValueIdentifier.ToLowerInvariant()));

            foreach (var value in chosen)
            {
                var target = value.OccurType == OccurType.Must ? all : any;

                target.AddRange(value.FieldValues.Select(ToCriterion));
            }
        }

        return new Client.CodeListSearchFilter { All = all, Any = any };
    }

    private static IEnumerable<FilterInputModel> Parse(IEnumerable<string> filters) =>
        filters
            .Select(x => JsonSerializer.Deserialize<FilterInputModel>(x, _json))
            .OfType<FilterInputModel>();

    private static Client.CodeListAnnotationCriterion ToCriterion(FilterDefinitionValueModel fieldValue) =>
        new()
        {
            Type = fieldValue.FieldName,
            Property = Property(fieldValue.FieldProperty),
            Value = string.IsNullOrWhiteSpace(fieldValue.FieldValue) ? null : fieldValue.FieldValue,
            Text = string.IsNullOrWhiteSpace(fieldValue.FieldValue)
                ? ToText(fieldValue.FieldValueMultilanguage)
                : null,
        };

    private static Client.CodeListAnnotationProperty? Property(string? fieldProperty) =>
        fieldProperty?.ToLowerInvariant() switch
        {
            "title" => Client.CodeListAnnotationProperty.Title,
            "identifier" => Client.CodeListAnnotationProperty.Identifier,
            _ => Client.CodeListAnnotationProperty.Type,
        };

    private static Client.MultiLanguageModel? ToText(DataAccess.Abstractions.MultiLanguageModel? text) =>
        text is null
            ? null
            : new Client.MultiLanguageModel
            {
                De = text.De,
                En = text.En,
                Fr = text.Fr,
                It = text.It,
                Rm = text.Rm,
            };
}
