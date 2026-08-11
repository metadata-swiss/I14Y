using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;

namespace Bfs.Iop.Core.Common.Api.Helpers;

public static class MimeTypeHelper
{
    public static string GetMimeType(RdfExportFormat format)
    {
        format.EnsureValueIsValid();

        return format switch
        {
            RdfExportFormat.RDF => "application/rdf+xml",
            RdfExportFormat.TTL => "text/turtle",
            _ => throw new NotSupportedException($"The format '{format}' is not supported.")
        };
    }
}
