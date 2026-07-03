using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DcatDatasetModel, Dataset>()
            .Map(dest => dest.AccessRights, src => src.AccessRights)
            .Map(dest => dest.ConfidentialityPerson, src => src.ConfidentialityPerson)
            .Map(dest => dest.ConformTos, src => src.ConformsTo)
            .Map(dest => dest.ContactPoints, src => src.ContactPoints)
            .Map(dest => dest.DataOwner, src => src.DataOwner)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Documents, src => src.Documentation)
            .Map(dest => dest.Distributions, src => src.Distributions)
            .Map(dest => dest.Frequency, src => src.Frequency)
            .Map(dest => dest.GeoIvId, src => src.GeoIvIds)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Image, src => src.Images)
            .Map(dest => dest.IsReferencedBy, src => src.IsReferencedBy)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.LandingPages, src => src.LandingPages)
            .Map(dest => dest.Languages, src => src.Languages.Select(x => x.Code))
            .Map(dest => dest.LastUpdated, src => src.Modified)
            .Ignore(dest => dest.NextVersions)
            .Ignore(dest => dest.PreviousVersion)
            .Map(dest => dest.ProcessId, src => src.ProcessId)
            .Map(dest => dest.PublicationLevel, src => src.PublicationLevel)
            .Map(dest => dest.PublicationLevelProposal, src => src.PublicationLevelProposal)
            .Map(dest => dest.Published, src => src.Issued)
            .Map(dest => dest.PublisherName, src => src.Publisher.Name)
            .Map(dest => dest.QualifiedAttribution, src => src.QualifiedAttributions)
            .Map(dest => dest.QualifiedAttributionComplement, src => src.QualifiedAttributionComplement)
            .Map(dest => dest.QualifiedRelation, src => src.QualifiedRelations)
            .Map(dest => dest.RegistrationStatus, src => src.RegistrationStatus)
            .Map(dest => dest.RegistrationStatusProposal, src => src.RegistrationStatusProposal)
            .Map(dest => dest.Relations, src => src.Relations)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.ResponsiblePersonDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.RetentionPeriod, src => src.RetentionPeriod)
            .Map(dest => dest.RetentionPeriodDescription, src => src.RetentionPeriodComplement)
            .Map(dest => dest.SpatialCoverages, src => src.Spatial)
            .Ignore(dest => dest.Status)
            .Map(dest => dest.System, src => src.System)
            .Map(dest => dest.TemporalCoverage, src => src.TemporalCoverage)
            .Map(dest => dest.Themes, src => src.Themes)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.VersionNotes, src => src.VersionNotes);

        config.NewConfig<DcatDatasetModel, DatasetVersionSummary>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version);

        config.NewConfig<DcatDatasetModel, IdLabel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Label, src => src.Title);
    }
}