using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.Geocat;

namespace Bfs.Iop.Admin.GeocatClient;

public class GeocatClientMappingProfile : Profile
{
    public GeocatClientMappingProfile()
        : base(nameof(GeocatClientMappingProfile))
    {
        CreateMap<ElasticSearch.SearchResult, GeocatSearchResult>()
            .ForMember(x => x.Count, x => x.MapFrom(src => src.Summary.TotalCount))
            .ForMember(x => x.From, x => x.Ignore())
            .ForMember(x => x.To, x => x.Ignore())
            ;

        CreateMap<ElasticSearch.SearchResultItem, GeocatSearchMetadata>()
            .ForMember(x => x.Abstract, x => x.MapFrom(src => src.Metadata.Abstract))
            .ForMember(x => x.Identifier, x => x.MapFrom(src => src.Metadata.Id))
            .ForMember(x => x.Link, x => x.MapFrom((src, dst, _, context) => (string)context.Items["LinkBaseUri"] + src.Metadata.Id))
            .ForMember(x => x.Title, x => x.MapFrom(src => src.Metadata.Title))
            ;

        CreateMap<ElasticSearch.Text, MultilingualText>()
            .ConvertUsing((src, dst, context) => new MultilingualText{
                { "de", src?.De ?? src?.Default ?? string.Empty },
                { "en", src?.En ?? src?.Default ?? string.Empty },
                { "fr", src?.Fr ?? src?.Default ?? string.Empty },
                { "it", src?.It ?? src?.Default ?? string.Empty }
            });
    }
}