using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AgentMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.Agent, IdentifierInputModel>()
            .Map(dest => dest.Identifier, src => src.Identifier);

        config.NewConfig<AgentModel, Models.Agent>()
            .Map(dest => dest.Classification, src => src.Classification)
            .Map(dest => dest.ContactPoint, src => src.ContactPoint)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.HomePage, src => src.HomePage)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Images, src => src.Images)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.PrefLabel, src => src.PrefLabel)
            .Map(dest => dest.Spatial, src => src.Spatial)
            .Map(dest => dest.SpatialCH, src => src.SpatialCH)
            .Map(dest => dest.SubAgents, src => src.SubAgents)
            .Ignore(dest => dest.SubAgentOf)
            .Map(dest => dest.System, src => src.System)
            .Map(dest => dest.Uid, src => src.Uid);

        config.NewConfig<AgentModel, IdNameModel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

        config.NewConfig<AgentModel, IdentifierInputModel>()
            .Map(dest => dest.Identifier, src => src.Identifier);
    }
}