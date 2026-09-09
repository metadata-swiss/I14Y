using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Bfs.Iop.Core.Serialization.Csv;

internal abstract class IopCsvSerializer
{
    protected static readonly CsvConfiguration _defaultConfiguration = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ",",
        MissingFieldFound = null, 
    };

    protected static void WriteCsvHeader<T>(
        IEnumerable<CsvExportFileColumn<T>> columns,
        CsvWriter csv) where T : class
    {
        foreach (string headerField in columns.Select(c => c.Header))
        {
            csv.WriteField(headerField);
        }
        csv.NextRecord();
    }

    protected static void WriteCsvContent<T>(
        IEnumerable<CsvExportFileColumn<T>> columns,
        IEnumerable<T> entries,
        CsvWriter csv) where T : class
    {
        foreach (var entry in entries)
        {
            foreach (var value in columns.Select(column => column.Accessor(entry)))
            {
                csv.WriteField(value, shouldQuote: !string.IsNullOrWhiteSpace(value));
            }
            csv.NextRecord();
        }
    }

    protected static void ExtractAndCompleteHeaderValues<T>(
        CsvReader csvReader,
        List<CsvImportFileColumn<T>> hardcodedHeaders,
        T item) where T : class
    {
        foreach (var hardcodedHeader in hardcodedHeaders)
        {
            if (csvReader.TryGetField(hardcodedHeader.Header, out string? fieldValue))
            {
                hardcodedHeader.PropertySetter(item, fieldValue!); // set value through column property setter action
            }
        }
    }
}
