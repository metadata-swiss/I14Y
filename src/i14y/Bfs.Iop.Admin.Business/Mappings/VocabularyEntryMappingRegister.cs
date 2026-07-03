using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class VocabularyEntryMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<VocabularyEntryModel, Models.VocabularyEntry>();

        config.NewConfig<Models.VocabularyEntry, VocabularyEntryModel>();

        config.NewConfig<Models.VocabularyEntry, CodeInputModel>()
            .Map(d => d.Code, s => s.Code);

        config.NewConfig<CodeInputModel, Models.VocabularyEntry>()
            .Map(d => d.Code, s => s.Code);
    }
}