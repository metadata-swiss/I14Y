using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CatalogEntryMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SearchResultModel, Models.CatalogEntry>()
            .Map(dest => dest.AccessRights, src => src.AccessRights)
            .Map(dest => dest.PublisherName, src => src.Publisher.Name)
            .Map(dest => dest.Structure, src => src.Structure)
            .Map(dest => dest.Formats, src => src.Formats)
            .Map(dest => dest.Identifiers, src => new[] { src.Identifier })
            .Map(dest => dest.PublicationLevel, src => src.PublicationLevel)
            .Map(dest => dest.PublicationLevelProposal, src => src.PublicationLevelProposal)
            .Map(dest => dest.RegistrationStatus, src => src.RegistrationStatus)
            .Map(dest => dest.RegistrationStatusProposal, src => src.RegistrationStatusProposal)
            .Map(dest => dest.Themes, src => src.Themes)
            .Map(dest => dest.System, src => src.System)
            .Map(
                dest => dest.ConceptValueType,
                src => src.ConceptType,
                src => src.ConceptType != null &&
                       Enum.IsDefined(typeof(ConceptType), src.ConceptType))
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.Version, src => src.Version);
    }
}