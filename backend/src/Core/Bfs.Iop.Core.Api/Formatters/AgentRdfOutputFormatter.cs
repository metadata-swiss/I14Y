using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Serialization.Rdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.Formatters;

/// <summary>
/// Base output formatter that serializes <see cref="AgentModel"/> (and sequences of it) to RDF,
/// selected via HTTP content negotiation on the existing agents GET endpoints.
/// </summary>
internal abstract class AgentRdfOutputFormatter : TextOutputFormatter
{
    private readonly CatalogExportFormat _format;

    protected AgentRdfOutputFormatter(CatalogExportFormat format, params string[] mediaTypes)
    {
        _format = format;

        foreach (var mediaType in mediaTypes)
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(mediaType));
        }

        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type? type) =>
        type is not null
        && (typeof(AgentModel).IsAssignableFrom(type)
            || typeof(IEnumerable<AgentModel>).IsAssignableFrom(type));

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(selectedEncoding, nameof(selectedEncoding));

        var serializer = context.HttpContext.RequestServices.GetRequiredService<IAgentRdfSerializer>();

        IEnumerable<AgentModel> agents = context.Object switch
        {
            AgentModel single => [single],
            IEnumerable<AgentModel> many => many,
            _ => []
        };

        var payload = serializer.Serialize(agents, _format);

        await context.HttpContext.Response.WriteAsync(payload, selectedEncoding);
    }
}
