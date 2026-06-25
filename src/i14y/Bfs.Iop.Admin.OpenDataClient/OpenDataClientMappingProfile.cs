using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.OpenData;

namespace Bfs.Iop.Admin.OpenDataClient;

public class OpenDataClientMappingProfile : Profile
{
    public OpenDataClientMappingProfile()
        : base(nameof(OpenDataClientMappingProfile))
    {
        CreateMap<PackageSearch.SearchResult, OpenDataSearchResult>()
            .ForMember(x => x.Items, x => x.MapFrom(src => src.Results))
            .ForMember(x => x.From, x => x.Ignore())
            .ForMember(x => x.To, x => x.Ignore())
            ;

        CreateMap<PackageSearch.SearchResultItem, OpenDataSearchResultItem>()
            .ForMember(x => x.Identifier, x => x.MapFrom(src => src.Id))
            .ForMember(x => x.Link, x => x.MapFrom((src, dst, _, context) => (string)context.Items["LinkBaseUri"] + src.LinkId))
            ;

        CreateMap<PackageSearch.Text, MultilingualText>()
            .ConvertUsing((src, dst, context) =>
            {
                var text = dst ?? new MultilingualText();
                if (!string.IsNullOrEmpty(src.De)) text["de"] = src.De;
                if (!string.IsNullOrEmpty(src.En)) text["en"] = src.En;
                if (!string.IsNullOrEmpty(src.Fr)) text["fr"] = src.Fr;
                if (!string.IsNullOrEmpty(src.It)) text["it"] = src.It;

                return text;
            });
    }
}