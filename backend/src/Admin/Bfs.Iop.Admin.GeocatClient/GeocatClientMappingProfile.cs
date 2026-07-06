using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;
using Mapster;
using System;

namespace Bfs.Iop.Admin.GeocatClient;

public sealed class GeocatClientMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ElasticSearch.SearchResult, GeocatSearchResult>()
            .Map(dest => dest.Count, src => src.Summary.TotalCount)
            .Ignore(dest => dest.From)
            .Ignore(dest => dest.To);

        config.NewConfig<ElasticSearch.SearchResultItem, GeocatSearchMetadata>()
            .Map(dest => dest.Abstract, src => src.Metadata.Abstract)
            .Map(dest => dest.Identifier, src => src.Metadata.Id)
            .Map(dest => dest.Link, src => new Uri(GetLinkBaseUri() + src.Metadata.Id))
            .Map(dest => dest.Title, src => src.Metadata.Title);

        config.NewConfig<ElasticSearch.Text, MultilingualText>()
            .MapWith(src => CreateMultilingualText(src));
    }

    private static string GetLinkBaseUri()
    {
        if (!MapContext.Current.Parameters.TryGetValue("LinkBaseUri", out var value))
        {
            throw new InvalidOperationException(
                "The mapping parameter 'LinkBaseUri' was not provided.");
        }

        return (string)value;
    }

    private static MultilingualText CreateMultilingualText(ElasticSearch.Text? src)
    {
        return new MultilingualText
        {
            { "de", src?.De ?? src?.Default ?? string.Empty },
            { "en", src?.En ?? src?.Default ?? string.Empty },
            { "fr", src?.Fr ?? src?.Default ?? string.Empty },
            { "it", src?.It ?? src?.Default ?? string.Empty }
        };
    }
}