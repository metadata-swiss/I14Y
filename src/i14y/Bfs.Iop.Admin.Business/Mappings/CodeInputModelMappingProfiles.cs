using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CodeInputModelMappingProfiles : Profile
{
    public CodeInputModelMappingProfiles() : base(nameof(CodeInputModelMappingProfiles))
    {
        CreateMap<VocabularyEntryModel, CodeInputModel>();
    }
}
