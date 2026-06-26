using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;
using System.Reflection.Metadata;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetMappingProfiles : Profile
{
    public DatasetMappingProfiles()
        : base(nameof(DatasetMappingProfiles))
    {
        CreateMap<DcatDatasetModel, Dataset>()
            .ForMember(d => d.AccessRights, opt => opt.MapFrom(s => s.AccessRights))
            .ForMember(d => d.ConfidentialityPerson, opt => opt.MapFrom(s => s.ConfidentialityPerson))
            .ForMember(d => d.ConformTos, opt => opt.MapFrom(s => s.ConformsTo))
            .ForMember(d => d.ContactPoints, opt => opt.MapFrom(s => s.ContactPoints))
            .ForMember(d => d.DataOwner, opt => opt.MapFrom(s => s.DataOwner))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documentation))
            .ForMember(d => d.Distributions, opt => opt.MapFrom(s => s.Distributions))
            .ForMember(d => d.Frequency, opt => opt.MapFrom(s => s.Frequency))
            .ForMember(d => d.GeoIvId, opt => opt.MapFrom(s => s.GeoIvIds))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Image, opt => opt.MapFrom(s => s.Images))
            .ForMember(d => d.IsReferencedBy, opt => opt.MapFrom(s => s.IsReferencedBy))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.LandingPages, opt => opt.MapFrom(s => s.LandingPages))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.Languages.Select(x => x.Code)))
            .ForMember(d => d.LastUpdated, opt => opt.MapFrom(s => s.Modified))
            .ForMember(d => d.NextVersions, opt => opt.Ignore())
            .ForMember(d => d.PreviousVersion, opt => opt.Ignore())
            .ForMember(d => d.ProcessId, opt => opt.MapFrom(s => s.ProcessId))
            .ForMember(d => d.PublicationLevel, opt => opt.MapFrom(s => s.PublicationLevel))
            .ForMember(d => d.PublicationLevelProposal, opt => opt.MapFrom(s => s.PublicationLevelProposal))
            .ForMember(d => d.Published, opt => opt.MapFrom(s => s.Issued))
            .ForMember(d => d.PublisherName, opt => opt.MapFrom(s => s.Publisher.Name))
            .ForMember(d => d.QualifiedAttribution, opt => opt.MapFrom(s => s.QualifiedAttributions))
            .ForMember(d => d.QualifiedAttributionComplement, opt => opt.MapFrom(s => s.QualifiedAttributionComplement))
            .ForMember(d => d.QualifiedRelation, opt => opt.MapFrom(s => s.QualifiedRelations))
            .ForMember(d => d.RegistrationStatus, opt => opt.MapFrom(s => s.RegistrationStatus))
            .ForMember(d => d.RegistrationStatusProposal, opt => opt.MapFrom(s => s.RegistrationStatusProposal))
            .ForMember(d => d.Relations, opt => opt.MapFrom(s => s.Relations))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.ResponsiblePersonDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.RetentionPeriod, opt => opt.MapFrom(s => s.RetentionPeriod))
            .ForMember(d => d.RetentionPeriodDescription, opt => opt.MapFrom(s => s.RetentionPeriodComplement))
            .ForMember(d => d.SpatialCoverages, opt => opt.MapFrom(s => s.Spatial))
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.System, opt => opt.MapFrom(s => s.System))
            .ForMember(d => d.TemporalCoverage, opt => opt.MapFrom(s => s.TemporalCoverage))
            .ForMember(d => d.Themes, opt => opt.MapFrom(s => s.Themes))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
            .ForMember(d => d.VersionNotes, opt => opt.MapFrom(s => s.VersionNotes));

        CreateMap<DcatDatasetModel, DatasetVersionSummary>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version));

        CreateMap<DcatDatasetModel, IdLabel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Title));
    }
}