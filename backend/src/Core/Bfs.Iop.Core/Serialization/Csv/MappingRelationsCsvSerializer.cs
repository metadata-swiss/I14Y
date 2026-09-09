using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using CsvHelper;
using System.Text;

namespace Bfs.Iop.Core.Serialization.Csv;

internal sealed class MappingRelationsCsvSerializer : IopCsvSerializer
{
    private const string SourceCodeUriColumnName = "Source_code_uri";
    private const string TargetCodeUriColumnName = "Target_code_uri";
    private const string RelationTypeColumnName = "Relation_type";

    public static ExportFile SerializeToFile(string fileName, IEnumerable<MappingRelationModel> data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var stream = new MemoryStream();

        var columns = PrepareColumns();

        using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);

        using var csv = new CsvWriter(writer, _defaultConfiguration);

        WriteCsvHeader(columns, csv);
        WriteCsvContent(columns, data, csv);

        writer.Flush();
        stream.Position = 0; // ensure the data is at the beginning

        return new ExportFile(stream, $"{fileName}.csv", "text/csv"); ;
    }

    public static IEnumerable<MappingRelationInputModel> DeserializeStreamData(Stream data)
    {
        using var reader = new StreamReader(data);
        using var csv = new CsvReader(reader, _defaultConfiguration);

        csv.Context.TypeConverterOptionsCache.GetOptions<string>().NullValues.AddRange(["", "NULL"]); // handle null values

        csv.Read();
        csv.ReadHeader();

        var items = new List<MappingRelationInputModel>();

        var headers = GetHardcodedHeaders();

        while (csv.Read())
        {
            var item = new MappingRelationInputModel()
            {
                RelationType = new CodeInputModel() { Code = string.Empty },
                Source = new UriInputModel() { Uri = string.Empty },
                Target = new UriInputModel() { Uri = string.Empty },
            };

            ExtractAndCompleteHeaderValues(csv, headers, item);

            items.Add(item);
        }

        return items;
    }

    private static IEnumerable<CsvExportFileColumn<MappingRelationModel>> PrepareColumns() =>
        [
            new(SourceCodeUriColumnName,  source => source.Source.Uri),
            new(TargetCodeUriColumnName,  source => source.Target.Uri),
            new(RelationTypeColumnName,   source => source.RelationType.Code),
        ];

    private static List<CsvImportFileColumn<MappingRelationInputModel>> GetHardcodedHeaders() => [
            //   *Column name*                    *Action to set the column value*
            new(SourceCodeUriColumnName,     (x, y) => x.Source = new UriInputModel() { Uri = y }),
            new(TargetCodeUriColumnName,     (x, y) => x.Target = new UriInputModel() { Uri = y }),
            new(RelationTypeColumnName,      (x, y) => x.RelationType = new CodeInputModel() { Code = y }),
        ];
}
