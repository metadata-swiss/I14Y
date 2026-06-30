using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Partner.Models.ConceptsInput;

namespace Bfs.Iop.Partner.Business.Mappings;

internal sealed class ConceptInputProfiles : Profile
{
    public ConceptInputProfiles() : base(nameof(ConceptInputProfiles))
    {
        CreateMap<ConceptInputBase, IopConceptInputModel>()
            .ForMember(d => d.Replaces, o => o.Ignore())
            .IncludeAllDerived();

        CreateMap<StringConceptInput, IopConceptInputModel>()
            .ForMember(dst => dst.ConceptType, options => options.MapFrom(_ => ConceptType.String));

        CreateMap<NumericConceptInput, IopConceptInputModel>()
            .ForMember(dst => dst.ConceptType, options => options.MapFrom(_ => ConceptType.Numeric));

        CreateMap<DateConceptInput, IopConceptInputModel>()
            .ForMember(dst => dst.ConceptType, options => options.MapFrom(_ => ConceptType.Date));

        CreateMap<CodeListConceptInput, IopConceptInputModel>()
            .ForMember(dst => dst.ConceptType, options => options.MapFrom(_ => ConceptType.CodeList));
    }
}
