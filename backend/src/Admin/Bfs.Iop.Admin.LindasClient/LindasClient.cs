using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bfs.Iop.Admin.Lindas.Abstractions;
using VDS.RDF.Query;

namespace Bfs.Iop.Admin.LindasClient;

internal sealed class LindasClient : ILindasClient
{
    private readonly Uri _ldBaseUrl;
    private readonly HttpClient _httpClient;
    private readonly Uri _queryUrl;
    private readonly SparqlQueryClient _sparqlQueryClient;

    public LindasClient(string? queryUrl, string? ldBaseUrl, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        if (!Uri.TryCreate(queryUrl, UriKind.Absolute, out var parsedQueryUrl))
        {
            throw new ArgumentException("QueryUrl must be configured as an absolute URI.", nameof(queryUrl));
        }

        if (!Uri.TryCreate(ldBaseUrl, UriKind.Absolute, out var parsedLdBaseUrl))
        {
            throw new ArgumentException("LdBaseUrl must be configured as an absolute URI.", nameof(ldBaseUrl));
        }

        _ldBaseUrl = new Uri($"{parsedLdBaseUrl.AbsoluteUri.TrimEnd('/')}/");
        _httpClient = httpClient;
        _queryUrl = parsedQueryUrl;
        _sparqlQueryClient = new SparqlQueryClient(_httpClient, _queryUrl);
    }

    public Task<Uri?> GetLinkAsync(
        LindasLinkType linkType,
        LindasResourceType resourceType,
        string identifier,
        string? version,
        CancellationToken cancellationToken) =>
        linkType switch
        {
            LindasLinkType.RdfLink => GetRdfLinkAsync(resourceType, identifier, version, cancellationToken),
            LindasLinkType.LdLink => GetLdLinkAsync(resourceType, identifier, version, cancellationToken),
            _ => throw new ArgumentException($"LINDAS link type '{linkType}' is not supported.", nameof(linkType))
        };

    private async Task<Uri?> GetRdfLinkAsync(
        LindasResourceType resourceType,
        string identifier,
        string? version,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        string existsQuery;
        string constructQuery;

        switch (resourceType)
        {
            case LindasResourceType.Concept:
                ArgumentException.ThrowIfNullOrWhiteSpace(version);
                existsQuery = LindasSparqlQueryHelper.BuildConceptExistsQuery(identifier, version);
                constructQuery = LindasSparqlQueryHelper.BuildConceptConstructQuery(identifier, version);
                break;
            case LindasResourceType.Dataset:
                existsQuery = LindasSparqlQueryHelper.BuildDatasetExistsQuery(identifier);
                constructQuery = LindasSparqlQueryHelper.BuildDatasetConstructQuery(identifier);
                break;
            default:
                throw new ArgumentException($"LINDAS resource type '{resourceType}' is not supported.", nameof(resourceType));
        }

        var result = await _sparqlQueryClient.QueryWithResultSetAsync(existsQuery, cancellationToken).ConfigureAwait(false);
        if (!result.Result)
        {
            return null;
        }

        var encodedQuery = Uri.EscapeDataString(constructQuery);
        return new Uri($"{_queryUrl.AbsoluteUri}?query={encodedQuery}&format=rdf");
    }

    private async Task<Uri?> GetLdLinkAsync(
        LindasResourceType resourceType,
        string identifier,
        string? version,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);

        var ldUri = resourceType switch
        {
            LindasResourceType.Concept => CreateConceptLdUri(identifier, version),
            LindasResourceType.Dataset => CreateDatasetLdUri(identifier),
            _ => throw new ArgumentException($"LINDAS resource type '{resourceType}' is not supported.", nameof(resourceType))
        };

        using var response = await _httpClient.GetAsync(
            ldUri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        return response.StatusCode == HttpStatusCode.OK ? ldUri : null;
    }

    private Uri CreateConceptLdUri(string identifier, string? version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        return new Uri(
            _ldBaseUrl,
            $"i14y/concept/{Uri.EscapeDataString(identifier)}/version/{Uri.EscapeDataString(version)}");
    }

    private Uri CreateDatasetLdUri(string identifier) => new(
        _ldBaseUrl,
        $"i14y/dataset/{Uri.EscapeDataString(identifier)}");
}