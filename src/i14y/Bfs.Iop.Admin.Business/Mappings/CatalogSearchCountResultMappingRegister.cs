using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Mapster;
using System.Collections.Generic;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CatalogSearchCountResultMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<string, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src)
            .Ignore(dest => dest.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultModel, FilterCountResult>()
            .Map(dest => dest.ConceptValueTypes, src =>
                src.ConceptValueTypes.Select(item => new FilterCountResultItem
                {
                    Reference = item.Value.ToString(),
                    Count = item.Count
                }))
            .Map(dest => dest.PublicationLevels, src =>
                src.PublicationLevels.Select(item => new FilterCountResultItem
                {
                    Count = item.Count,
                    Reference = item.Value.ToString(),
                }))
            .Map(dest => dest.PublicationLevelProposals, src =>
                src.PublicationLevelProposals == null
                    ? new List<FilterCountResultItem>()
                    : src.PublicationLevelProposals.Select(item => new FilterCountResultItem
                    {
                        Count = item.Count,
                        Reference = item.Value.ToString(),
                    }))
            .Map(dest => dest.RegistrationStatuses, src =>
                src.RegistrationStatuses.Select(item => new FilterCountResultItem
                {
                    Count = item.Count,
                    Reference = item.Value.ToString(),
                }))
            .Map(dest => dest.RegistrationStatusProposals, src =>
                src.RegistrationStatusProposals == null
                    ? new List<FilterCountResultItem>()
                    : src.RegistrationStatusProposals.Select(item => new FilterCountResultItem
                    {
                        Count = item.Count,
                        Reference = item.Value.ToString(),
                    }))
            .Map(dest => dest.Structures, src =>
                src.Structures == null
                    ? new List<FilterCountResultItem>()
                    : src.Structures.Select(item => new FilterCountResultItem
                    {
                        Count = item.Count,
                        Reference = item.Value.ToString(),
                    }))
            .Map(dest => dest.Types, src =>
                src.Types == null
                    ? new List<FilterCountResultItem>()
                    : src.Types.Select(item => new FilterCountResultItem
                    {
                        Count = item.Count,
                        Reference = item.Value.ToString(),
                    }))
            .Map(dest => dest.TotalDocCount, src => src.TotalDocCount);

        config.NewConfig<SearchCountResultItem<string>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<AgentModel>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.Identifier)
            .Map(dest => dest.Label, src => src.Value.Name);

        config.NewConfig<SearchCountResultItem<VocabularyEntryModel>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.Code)
            .Map(dest => dest.Label, src => src.Value.Name);

        config.NewConfig<SearchCountResultItem<PublicationLevel>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<RegistrationStatus>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<PublicationLevel?>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<RegistrationStatus?>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<SearchStructureOption>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<SearchStructureOption?>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<SearchResourceType>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);

        config.NewConfig<SearchCountResultItem<SearchResourceType?>, FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Value.ToString())
            .Map(dest => dest.Count, src => src.Count)
            .Ignore(dest => dest.Label);
    }
}