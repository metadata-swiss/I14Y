using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal sealed class DistributionMappingProfiles : Profile
{
    public DistributionMappingProfiles()
        : base(nameof(DistributionMappingProfiles))
    {
        CreateMap<Models.Distribution, DcatDistributionInputModel>()
            .ForMember(d => d.AccessUrl, opt =>
            {
                opt.PreCondition(s => s.AccessUrls.Any());
                opt.MapFrom(s => s.AccessUrls.First());
            })
            .ForMember(d => d.Availability, opt => opt.MapFrom(s => s.Availability))
            .ForMember(d => d.ByteSize, opt => opt.MapFrom(s => s.ByteSize))
            .ForMember(d => d.Checksum, opt => opt.MapFrom(s => s.Checksum))
            .ForMember(d => d.ConformsTo, opt => opt.MapFrom(s => s.ConformTos))
            .ForMember(d => d.Coverage, opt => opt.MapFrom(s => s.Coverage))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documentation, opt => opt.MapFrom(s => s.Documentation))
            .ForMember(d => d.DownloadUrl, opt =>
            {
                opt.PreCondition(s => s.DownloadUrls.Any());
                opt.MapFrom(s => s.DownloadUrls.First());
            })
            .ForMember(d => d.Format, opt => opt.MapFrom(s => s.Format))
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Image))
            .ForMember(d => d.Issued, opt => opt.MapFrom(s => s.Published))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.Languages.Select(l => new CodeInputModel() { Code = l })))
            .ForMember(d => d.License, opt => opt.MapFrom(s => s.License))
            .ForMember(d => d.MediaType, opt => opt.MapFrom(s => s.MediaType))
            .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.LastUpdated))
            .ForMember(d => d.PackagingFormat, opt => opt.MapFrom(s => s.PackagingFormat))
            .ForMember(d => d.Rights, opt => opt.MapFrom(s => s.Rights))
            .ForMember(d => d.SpatialResolution, opt =>
            {
                opt.PreCondition(s => s.SpatialResolution.Any() && decimal.TryParse(s.SpatialResolution.First(), out _));
                opt.MapFrom(s => decimal.Parse(s.SpatialResolution.First()));
            })
            .ForMember(d => d.TemporalResolution, opt => opt.MapFrom(s => s.TemporalResolution))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            ;
    }
}