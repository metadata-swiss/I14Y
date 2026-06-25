using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;
using Bfs.Iop.Admin.Models.OpenData;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class MetaSearchMappingProfiles : Profile
{
    public MetaSearchMappingProfiles()
        : base(nameof(MetaSearchMappingProfiles))
    {
        CreateMap<MultilingualText, LocalizedText>().ConvertUsing<MultilingualTextToLocalizedTextConverter>();

        CreateMap<GeocatSearchResult, PagedResult<MetaSearchResultItem>>()
            .ForMember(x => x.PageSize, x => x.MapFrom((src, dst, _, context) =>
            {
                if (context.Items.TryGetValue("pageSize", out var pageSize) && pageSize is int result) return result;

                return src.To + 1 - src.From;
            }))
            .ForMember(x => x.Page, x => x.MapFrom((src, dst, _, context) =>
            {
                if (context.Items.TryGetValue("pageSize", out var pageSize) && pageSize is int result) return (src.From / result) + 1;

                return 1 + (src.From - 1) / (src.To + 1 - src.From);
            }))
            .ForMember(x => x.TotalCount, x => x.MapFrom(src => src.Count))
            .ForMember(x => x.Results, x => x.MapFrom(src => src.Metadata));

        CreateMap<GeocatSearchMetadata, MetaSearchResultItem>()
            .ForMember(x => x.Description, x => x.MapFrom(src => src.Abstract));

        CreateMap<OpenDataSearchResult, PagedResult<MetaSearchResultItem>>()
            .ForMember(x => x.PageSize, x => x.MapFrom((src, dst, _, context) =>
            {
                if (context.Items.TryGetValue("pageSize", out var pageSize) && pageSize is int result) return result;

                return src.To + 1 - src.From;
            }))
            .ForMember(x => x.Page, x => x.MapFrom((src, dst, _, context) =>
            {
                if (context.Items.TryGetValue("pageSize", out var pageSize) && pageSize is int result) return (src.From / result) + 1;

                return 1 + (src.From - 1) / (src.To + 1 - src.From);
            }))
            .ForMember(x => x.TotalCount, x => x.MapFrom(src => src.Count))
            .ForMember(x => x.Results, x => x.MapFrom(src => src.Items));

        CreateMap<OpenDataSearchResultItem, MetaSearchResultItem>();
    }
}
