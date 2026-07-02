using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.OpenData;
using Mapster;
using System;

namespace Bfs.Iop.Admin.OpenDataClient;

public sealed class OpenDataClientMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PackageSearch.SearchResult, OpenDataSearchResult>()
            .Map(dest => dest.Items, src => src.Results)
            .Ignore(dest => dest.From)
            .Ignore(dest => dest.To);

        config.NewConfig<PackageSearch.SearchResultItem, OpenDataSearchResultItem>()
            .Map(dest => dest.Identifier, src => src.Id)
            .Map(dest => dest.Link, src => GetLinkBaseUri() + src.LinkId);

        config.NewConfig<PackageSearch.Text, MultilingualText>()
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

    private static MultilingualText CreateMultilingualText(PackageSearch.Text? src)
    {
        var text = new MultilingualText();

        if (src == null)
            return text;

        if (!string.IsNullOrEmpty(src.De))
            text["de"] = src.De;

        if (!string.IsNullOrEmpty(src.En))
            text["en"] = src.En;

        if (!string.IsNullOrEmpty(src.Fr))
            text["fr"] = src.Fr;

        if (!string.IsNullOrEmpty(src.It))
            text["it"] = src.It;

        return text;
    }
}