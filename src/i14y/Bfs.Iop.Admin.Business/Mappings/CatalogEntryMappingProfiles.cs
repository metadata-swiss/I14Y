using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CatalogEntryMappingProfiles : Profile
{
    public CatalogEntryMappingProfiles()
        : base(nameof(CatalogEntryMappingProfiles))
    {
        CreateMap<SearchResultModel, Models.CatalogEntry>()
            .ForMember(t => t.AccessRights, opt => opt.MapFrom(s => s.AccessRights))
            .ForMember(t => t.PublisherName, opt => opt.MapFrom(s => s.Publisher.Name))
            .ForMember(t => t.Structure, opt => opt.MapFrom(s => s.Structure))
            .ForMember(t => t.Formats, opt => opt.MapFrom(s => s.Formats))
            .ForMember(t => t.Identifiers, opt => opt.MapFrom(s => new[] { s.Identifier }))
            .ForMember(t => t.PublicationLevel, opt => opt.MapFrom(s => s.PublicationLevel.ToString()))
            .ForMember(t => t.PublicationLevelProposal, opt => opt.MapFrom(s => s.PublicationLevelProposal.ToString()))
            .ForMember(t => t.RegistrationStatus, opt => opt.MapFrom(s => s.RegistrationStatus.ToString()))
            .ForMember(t => t.RegistrationStatusProposal, opt => opt.MapFrom(s => s.RegistrationStatusProposal.ToString()))
            .ForMember(t => t.Themes, opt => opt.MapFrom(s => s.Themes))
            .ForMember(t => t.System, opt => opt.MapFrom(s => s.System))
            .ForMember(t => t.ConceptValueType, opt =>
            {
                opt.PreCondition(s => s.ConceptType is not null && Enum.IsDefined(typeof(ConceptType), s.ConceptType));
                opt.MapFrom(s => s.ConceptType);
            })
            .ForMember(t => t.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(t => t.ValidTo, opt => opt.MapFrom(s => s.ValidTo))
            .ForMember(t => t.Version, opt => opt.MapFrom(s => s.Version))
            ;
    }
}