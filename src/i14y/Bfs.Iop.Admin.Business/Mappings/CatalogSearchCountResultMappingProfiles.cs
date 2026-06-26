using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using System.Collections.Generic;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CatalogSearchCountResultMappingProfiles : Profile
{
    public CatalogSearchCountResultMappingProfiles()
        : base(nameof(CatalogSearchCountResultMappingProfiles))
    {
        CreateMap<string, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.Count, opt => opt.Ignore())
            .ForMember(dest => dest.Label, opt => opt.Ignore())
            ;

        CreateMap<SearchCountResultModel, FilterCountResult>()
           .ForMember(x => x.ConceptValueTypes, x => x.MapFrom(src =>
                src.ConceptValueTypes.Select(x => new FilterCountResultItem
                {
                    Reference = x.Value.ToString(),
                    Count = x.Count
                })))
           .ForMember(x => x.PublicationLevels, x => x.MapFrom(src =>
               src.PublicationLevels.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.PublicationLevelProposals, x => x.MapFrom(src =>
               src.PublicationLevelProposals == null ? new List<FilterCountResultItem>() :
               src.PublicationLevelProposals.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.RegistrationStatuses, x => x.MapFrom(src =>
               src.RegistrationStatuses.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.RegistrationStatusProposals, x => x.MapFrom(src =>
               src.RegistrationStatusProposals == null ? new List<FilterCountResultItem>() :
               src.RegistrationStatusProposals.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.Structures, x => x.MapFrom(src =>
               src.Structures == null ? new List<FilterCountResultItem>() :
               src.Structures.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.Types, x => x.MapFrom(src =>
               src.Types == null ? new List<FilterCountResultItem>() :
               src.Types.Select(item => new FilterCountResultItem
               {
                   Count = item.Count,
                   Reference = item.Value.ToString(),
               })))
           .ForMember(x => x.TotalDocCount, x => x.MapFrom(src => src.TotalDocCount));
        ;

        CreateMap<SearchCountResultItem<string>, FilterCountResultItem>()
            .ForMember(x => x.Reference, x => x.MapFrom(src => src.Value))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<AgentModel>, FilterCountResultItem>()
            .ForMember(x => x.Reference, x => x.MapFrom(src => src.Value.Identifier))
            .ForMember(x => x.Label, x => x.MapFrom(src => src.Value.Name))
            ;

        CreateMap<SearchCountResultItem<VocabularyEntryModel>, FilterCountResultItem>()
            .ForMember(x => x.Reference, x => x.MapFrom(src => src.Value.Code))
            .ForMember(x => x.Label, x => x.MapFrom(src => src.Value.Name))
            ;

        CreateMap<SearchCountResultItem<PublicationLevel>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<RegistrationStatus>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<PublicationLevel?>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<RegistrationStatus?>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<SearchStructureOption>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
            ;

        CreateMap<SearchCountResultItem<SearchStructureOption?>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore());

        CreateMap<SearchCountResultItem<SearchResourceType>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore());

        CreateMap<SearchCountResultItem<SearchResourceType?>, FilterCountResultItem>()
            .ForMember(dest => dest.Reference, opt => opt.MapFrom(src => src.Value.ToString()))
            .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
            .ForMember(x => x.Label, x => x.Ignore())
    ;
    }
}