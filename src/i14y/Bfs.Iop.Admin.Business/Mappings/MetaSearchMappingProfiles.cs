using Bfs.Iop.Admin.Business.Services;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;
using Bfs.Iop.Admin.Models.OpenData;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

public sealed class MetaSearchMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MultilingualText, LocalizedText>()
            .MapWith(src => Localize(MapContext.Current.GetService<ILocalizerService>(), src));

        config.NewConfig<GeocatSearchResult, PagedResult<MetaSearchResultItem>>()
            .Map(dest => dest.PageSize,
                src => GetPageSize(src))
            .Map(dest => dest.Page,
                src => GetPage(src))
            .Map(dest => dest.TotalCount,
                src => src.Count)
            .Map(dest => dest.Results,
                src => src.Metadata);

        config.NewConfig<GeocatSearchMetadata, MetaSearchResultItem>()
            .Map(dest => dest.Description,
                src => src.Abstract);

        config.NewConfig<OpenDataSearchResult, PagedResult<MetaSearchResultItem>>()
            .Map(dest => dest.PageSize,
                src => GetPageSize(src))
            .Map(dest => dest.Page,
                src => GetPage(src))
            .Map(dest => dest.TotalCount,
                src => src.Count)
            .Map(dest => dest.Results,
                src => src.Items);

        config.NewConfig<OpenDataSearchResultItem, MetaSearchResultItem>();
    }

    private LocalizedText Localize(ILocalizerService localizerService, MultilingualText src)
    {
        if (src == null)
        {
            return null!;
        }

        if (!MapContext.Current.Parameters.TryGetValue("language", out var languageValue))
        {
            throw new ArgumentException(
                "The converter needs the context parameter 'language' in order to map a MultilingualText to a LocalizedText.");
        }

        return localizerService.GetLocalizedText(src, (string)languageValue);
    }

    private static int GetPageSize(dynamic src)
    {
        if (MapContext.Current!.Parameters.TryGetValue("pageSize", out var value))
            return (int)value;

        return src.To + 1 - src.From;
    }

    private static int GetPage(dynamic src)
    {
        if (MapContext.Current!.Parameters.TryGetValue("pageSize", out var value))
            return (src.From / (int)value) + 1;

        return 1 + (src.From - 1) / (src.To + 1 - src.From);
    }
}