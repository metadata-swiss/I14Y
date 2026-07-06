using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CodeListEntryModelMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CodelistEntryInput, CodeListEntryInputModel>()
            .Map(dest => dest.Code, src => src.Value)
            .Map(dest => dest.ParentCode, src => src.ParentCode)
            .Ignore(dest => dest.Annotations)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo);

        config.NewConfig<CodeListEntryModel, CodeListEntryDetail>()
            .Map(dest => dest.Annotations, src => src.Annotations)
            .Ignore(dest => dest.HasChildren)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ParentCode, src => src.ParentCode)
            .Map(dest => dest.Value, src => src.Code)
            .Ignore(dest => dest.ConceptId)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo);

        config.NewConfig<CodeListEntryModel, CodeListEntryInputModel>()
            .Map(dest => dest.Annotations, src => src.Annotations)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ParentCode, src => src.ParentCode)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo);

        config.NewConfig<CodeListEntryModel, CodelistEntryInput>()
            .Ignore(dest => dest.ConceptId)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ParentCode, src => src.ParentCode)
            .Map(dest => dest.Value, src => src.Code)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo);
    }
}