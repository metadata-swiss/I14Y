using Bfs.Iop.Admin.Business.Services;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;
using Bfs.Iop.Admin.Models.OpenData;
using Mapster;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

public sealed class MetaSearchMapsterRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MultilingualText, LocalizedText>()
            .MapWith(src => Localize(MapContext.Current.GetService<ILocalizerService>(), src));

        config.NewConfig<GeocatSearchMetadata, MetaSearchResultItem>()
            .Map(dest => dest.Description,
                src => src.Abstract);

        config.NewConfig<OpenDataSearchResultItem, MetaSearchResultItem>();
    }

    private static LocalizedText Localize(ILocalizerService localizerService, MultilingualText src)
    {
        if (src is null || MapContext.Current is null)
        {
            return null!;
        }

        if (!MapContext.Current.Parameters.TryGetValue("language", out var languageValue))
        {
            throw new ArgumentException(
                "The converter needs the context parameter 'language' in order to map a MultilingualText to a LocalizedText.");
        }

        return localizerService.GetLocalizedText(src, (string)languageValue)!;
    }
}