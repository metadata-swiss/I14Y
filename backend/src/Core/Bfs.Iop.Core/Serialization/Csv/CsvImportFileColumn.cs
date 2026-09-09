using System.Diagnostics;

namespace Bfs.Iop.Core.Serialization.Csv;

[DebuggerDisplay("Header={Header}")]
internal sealed record CsvImportFileColumn<TSourceType>(
    string Header,
    Action<TSourceType, string> PropertySetter) where TSourceType : class;