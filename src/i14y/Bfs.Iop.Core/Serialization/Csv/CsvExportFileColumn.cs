using System.Diagnostics;

namespace Bfs.Iop.Core.Serialization.Csv;

[DebuggerDisplay("Header={Header}")]
internal sealed record CsvExportFileColumn<TSourceType>(
    string Header,
    Func<TSourceType, string> Accessor) where TSourceType : class;
