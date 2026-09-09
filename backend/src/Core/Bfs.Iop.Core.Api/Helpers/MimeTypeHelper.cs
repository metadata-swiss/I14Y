using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Core.Abstractions.Models;
using System;

namespace Bfs.Iop.Core.Api.Helpers;

public static class MimeTypeHelper
{
    public static string GetMimeType(RdfExportFormat format)
    {
        format.EnsureValueIsValid();

        return format switch
        {
            RdfExportFormat.RDF => "application/rdf+xml",
            RdfExportFormat.TTL => "application/x-turtle",
            _ => throw new NotSupportedException($"The format '{format}' is not supported.")
        };
    }
}
