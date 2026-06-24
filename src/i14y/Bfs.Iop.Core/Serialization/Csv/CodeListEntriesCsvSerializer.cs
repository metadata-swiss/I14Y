using Bfs.Iop.Core.Abstractions.Models;
using CsvHelper;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Core.Serialization.Csv;

internal sealed class CodeListEntriesCsvSerializer : IopCsvSerializer
{
    public static ExportFile SerializeToFile(string fileName, IEnumerable<CodeListEntryModel> data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var stream = new MemoryStream();

        var columns = PrepareColumns(data);

        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        using var csv = new CsvWriter(writer, _defaultConfiguration);

        WriteCsvHeader(columns, csv);
        WriteCsvContent(columns, data, csv);

        writer.Flush();
        stream.Position = 0; // ensure the data is at the beginning

        return new ExportFile(stream, $"{fileName}.csv", "text/csv"); ;
    }

    public static IEnumerable<CodeListEntryInputModel> DeserializeStreamData(Stream data)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        using var reader = new StreamReader(data);
        using var csv = new CsvReader(reader, _defaultConfiguration);

        csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.AddRange(["", "NULL"]); // handle null values

        csv.Read();
        csv.ReadHeader();

        var hardcodedHeaders = GetHardcodedHeaders();

        EnsureHeadersAreConformToCsvImportPattern(csv.HeaderRecord);

        var entries = new List<CodeListEntryInputModel>();

        while (csv.Read())
        {
            var annotationTypeHeadersGrouped = ExtractDynamicAnnotationHeaders(csv);

            var codeListEntryImport = new CodeListEntryInputModel()
            {
                Code = string.Empty, //this is an invalid value which must be changed during the processing
                Name = new MultiLanguageModel(),
                Description = new MultiLanguageModel(),
            };

            ExtractAndCompleteHeaderValues(csv, hardcodedHeaders, codeListEntryImport);

            codeListEntryImport.Annotations = ExtractAnnotations(csv, annotationTypeHeadersGrouped);

            entries.Add(codeListEntryImport);
        }

        return entries;
    }

    private static IEnumerable<CsvExportFileColumn<CodeListEntryModel>> PrepareColumns(IEnumerable<CodeListEntryModel> items)
    {
        List<CsvExportFileColumn<CodeListEntryModel>> columns =
        [
                new("Code",           source => source.Code),
                new("ParentCode",     source => source.ParentCode       ?? string.Empty),
                new("Name_de",        source => source.Name.De          ?? string.Empty),
                new("Name_fr",        source => source.Name.Fr          ?? string.Empty),
                new("Name_it",        source => source.Name.It          ?? string.Empty),
                new("Name_rm",        source => source.Name.Rm          ?? string.Empty),
                new("Name_en",        source => source.Name.En          ?? string.Empty),
                new("Description_de", source => source.Description?.De  ?? string.Empty),
                new("Description_fr", source => source.Description?.Fr  ?? string.Empty),
                new("Description_it", source => source.Description?.It  ?? string.Empty),
                new("Description_rm", source => source.Description?.Rm  ?? string.Empty),
                new("Description_en", source => source.Description?.En  ?? string.Empty),
                .. PrepareAnnotationColumns(items),
        ];

        return columns;
    }

    private static IEnumerable<CsvExportFileColumn<CodeListEntryModel>> PrepareAnnotationColumns(IEnumerable<CodeListEntryModel> codelistEntries)
    {
        List<CsvExportFileColumn<CodeListEntryModel>> annotationColumns = new();

        var annotationCountByGroups = codelistEntries
            .SelectMany(codelistEntry => (codelistEntry.Annotations ?? []).Where(i => !string.IsNullOrWhiteSpace(i.Type))) // handle null Annotations
            .GroupBy(x => x.Type!) // group the annotations by 'Type'
            .ToDictionary( // create a dictionary
                group => group.Key, // the key is the grouped key --> 'Type'
                group => codelistEntries
                    .Max( // calculate maximum amount of annotation based by group.Key --> 'Type'
                        codelistEntry => (codelistEntry.Annotations ?? [])
                            .Count(annotation => annotation.Type == group.Key)));

        foreach (var annotationGroup in annotationCountByGroups)
        {
            if (annotationGroup.Value == 1)
            {               
                // only one identical annotation
                annotationColumns.AddRange(GetAnnotationColumns(annotationGroup.Key));
                continue;
            }

            for (int i = 0; i < annotationGroup.Value; i++)
            {
                // multiple columns with same type
                annotationColumns.AddRange(GetAnnotationColumns(annotationGroup.Key, i));
            }
        }
        return annotationColumns;
    }

    private static List<CsvExportFileColumn<CodeListEntryModel>> GetAnnotationColumns(string annotationType, int? index = null)
    {
        int skip = index ?? 0;

        AnnotationModel? tryGetSingleAnnotation(IEnumerable<AnnotationModel> annotationApiViews)
        {
            return annotationApiViews.Where(a => a.Type == annotationType).Skip(skip).FirstOrDefault();
        };

        List<CsvExportFileColumn<CodeListEntryModel>> annotationColumns = [];

        string annotationPrefix = $"Annotation_";
        annotationPrefix += index.HasValue ? $"{annotationType}{index.Value + 1}" : $"{annotationType}";

        annotationColumns.Add(new($"{annotationPrefix}_Type", e => tryGetSingleAnnotation(e.Annotations)?.Type ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Title", e => tryGetSingleAnnotation(e.Annotations)?.Title ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_URI", e => tryGetSingleAnnotation(e.Annotations)?.Uri ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Identifier", e => tryGetSingleAnnotation(e.Annotations)?.Identifier ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Text_de", e => tryGetSingleAnnotation(e.Annotations)?.Text?.De ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Text_fr", e => tryGetSingleAnnotation(e.Annotations)?.Text?.Fr ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Text_it", e => tryGetSingleAnnotation(e.Annotations)?.Text?.It ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Text_rm", e => tryGetSingleAnnotation(e.Annotations)?.Text?.Rm ?? string.Empty));
        annotationColumns.Add(new($"{annotationPrefix}_Text_en", e => tryGetSingleAnnotation(e.Annotations)?.Text?.En ?? string.Empty));

        return annotationColumns;
    }

    private static List<CsvImportFileColumn<CodeListEntryInputModel>> GetHardcodedHeaders() => [
            //   *Column name*     *Action to set the column value*
            new( "Code",           (x, y) => x.Code = y            ),
            new( "ParentCode",     (x, y) => x.ParentCode = string.IsNullOrWhiteSpace(y) ? null : y),
            new( "Name_de",        (x, y) => x.Name.De = y         ),
            new( "Name_fr",        (x, y) => x.Name.Fr = y         ),
            new( "Name_it",        (x, y) => x.Name.It = y         ),
            new( "Name_rm",        (x, y) => x.Name.Rm = y         ),
            new( "Name_en",        (x, y) => x.Name.En = y         ),
            new( "Description_de", (x, y) => x.Description!.De = y ),
            new( "Description_fr", (x, y) => x.Description!.Fr = y ),
            new( "Description_it", (x, y) => x.Description!.It = y ),
            new( "Description_rm", (x, y) => x.Description!.Rm = y ),
            new( "Description_en", (x, y) => x.Description!.En = y ),
        ];

    private static void EnsureHeadersAreConformToCsvImportPattern(IEnumerable<string>? headers)
    {
        ArgumentNullException.ThrowIfNull(headers, nameof(headers));

        string[] annotationPatterns =
        [
            "^(Annotation_).+(_Type)$",
            "^(Annotation_).+(_Title)$",
            "^(Annotation_).+(_URI)$",
            "^(Annotation_).+(_Identifier)$",
            "^(Annotation_).+(_Text_de)$",
            "^(Annotation_).+(_Text_fr)$",
            "^(Annotation_).+(_Text_it)$",
            "^(Annotation_).+(_Text_rm)$",
            "^(Annotation_).+(_Text_en)$"
        ];

        foreach (string header in headers)
        {
            if (GetHardcodedHeaders().Select(i => i.Header).Contains(header)
                || annotationPatterns.Any(pattern => Regex.IsMatch(header, pattern)))
            {
                continue;
            }

            throw new InvalidOperationException($"The column '{header}' is not recognized in the csv import pattern.");
        }
    }

    private static ReadOnlyDictionary<string, int> ExtractDynamicAnnotationHeaders(CsvReader csvReader)
    {
        const string annotationHeaderSearchStart = "Annotation_";
        const string annotationHeaderSearchEnd = "_Type";

        if (csvReader.HeaderRecord is null)
        {
            throw new InvalidOperationException($"HeaderRecord is null!");
        }

        var annotationTypeHeadersGrouped = csvReader.HeaderRecord
            .Where(header =>
                header.StartsWith(annotationHeaderSearchStart, StringComparison.InvariantCultureIgnoreCase)
                && header.EndsWith(annotationHeaderSearchEnd, StringComparison.InvariantCultureIgnoreCase))
            .Select(header => csvReader.GetField(header) ?? throw new InvalidOperationException($"Missing value for field: {header}"))
            .GroupBy(h => h)
            .ToDictionary(group => group.Key, group => group.Count())
            .AsReadOnly();

        return annotationTypeHeadersGrouped;
    }

    private static List<AnnotationInputModel> ExtractAnnotations(
        CsvReader csvReader,
        IReadOnlyDictionary<string, int> annotationTypeHeadersGrouped
        )
    {
        var annotations = new List<AnnotationInputModel>();

        foreach (var headerGroup in annotationTypeHeadersGrouped)
        {
            for (var i = 1; i <= headerGroup.Value; i++)
            {
                var headerTypeName = headerGroup.Value == 1
                    ? headerGroup.Key // single header of specific type
                    : $"{headerGroup.Key}{i}"; // multiple headers of specific type numbered

                if (TryExtractAnnotation(csvReader, headerTypeName, out var annotation))
                {
                    annotations.Add(annotation!);
                }
            }
        }

        return annotations;
    }

    private static bool TryExtractAnnotation(CsvReader csvReader, string headerTypeName, out AnnotationInputModel? annotation)
    {
        var type = csvReader.GetField<string>($"Annotation_{headerTypeName}_Type");

        annotation = string.IsNullOrWhiteSpace(type)
            ? null
            : new()
            {
                Identifier = csvReader.GetField<string>($"Annotation_{headerTypeName}_Identifier"),
                Text = new MultiLanguageModel()
                {
                    De = csvReader.GetField<string>($"Annotation_{headerTypeName}_Text_de"),
                    Fr = csvReader.GetField<string>($"Annotation_{headerTypeName}_Text_fr"),
                    It = csvReader.GetField<string>($"Annotation_{headerTypeName}_Text_it"),
                    Rm = csvReader.GetField<string>($"Annotation_{headerTypeName}_Text_rm"),
                    En = csvReader.GetField<string>($"Annotation_{headerTypeName}_Text_en"),
                },
                Title = csvReader.GetField<string>($"Annotation_{headerTypeName}_Title"),
                Type = type,
                Uri = csvReader.GetField<string>($"Annotation_{headerTypeName}_URI"),
            };

        return annotation is not null;
    }
}
