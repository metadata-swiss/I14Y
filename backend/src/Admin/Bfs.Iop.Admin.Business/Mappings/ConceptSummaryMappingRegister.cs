using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal sealed class ConceptSummaryMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VocabularyEntryModel, Models.FilterCountResultItem>()
            .Map(dest => dest.Reference, src => src.Code)
            .Ignore(dest => dest.Count)
            .Map(dest => dest.Label, src => src.Name);
    }
}