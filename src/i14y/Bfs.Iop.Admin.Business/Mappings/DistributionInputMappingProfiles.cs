using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal sealed class DistributionInputMappingProfiles : Profile
{
    public DistributionInputMappingProfiles()
        : base(nameof(DistributionInputMappingProfiles))
    {
        CreateMap<Models.DistributionInput, DcatDistributionInputModel>()
            .ForMember(d => d.AccessServices, opt => opt.Ignore())
            .ForMember(d => d.AccessUrl, opt => opt.MapFrom(s => s.AccessUrl))
            .ForMember(d => d.Availability, opt => opt.MapFrom(s => s.Availability))
            .ForMember(d => d.ByteSize, opt => opt.MapFrom(s => s.ByteSize))
            .ForMember(d => d.Checksum, opt => opt.MapFrom(s => s.Checksum))
            .ForMember(d => d.ConformsTo, opt => opt.MapFrom(s => s.ConformTos))
            .ForMember(d => d.Coverage, opt => opt.MapFrom(s => s.Coverage))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documentation, opt => opt.MapFrom(s => s.Documents))
            .ForMember(d => d.DownloadUrl, opt =>
            {
                opt.PreCondition(s => s.AccessUrl.IsDownload);
                opt.MapFrom(s => s.AccessUrl);
            })
            .ForMember(d => d.Format, opt => opt.MapFrom(s => s.Format))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Image))
            .ForMember(d => d.Issued, opt => opt.MapFrom(s => s.Published))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.Languages.Select(x => new CodeInputModel() { Code = x })))
            .ForMember(d => d.License, opt => opt.MapFrom(s => s.License))
            .ForMember(d => d.MediaType, opt => opt.MapFrom(s => s.MediaType))
            .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.LastUpdated))
            .ForMember(d => d.PackagingFormat, opt => opt.MapFrom(s => s.PackagingFormat))
            .ForMember(d => d.Rights, opt =>
            {
                opt.PreCondition(s => s.Rights is not null);
                opt.MapFrom(s => s.Rights!.Code);
            })
            .ForMember(d => d.SpatialResolution, opt =>
            {
                opt.PreCondition(s => s.SpatialResolution is not null && s.SpatialResolution.Any() && decimal.TryParse(s.SpatialResolution.First(), out _));
                opt.MapFrom(s => s.SpatialResolution.First());
            })
            .ForMember(d => d.TemporalResolution, opt => opt.MapFrom(s => s.TemporalResolution))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title));
    }
}