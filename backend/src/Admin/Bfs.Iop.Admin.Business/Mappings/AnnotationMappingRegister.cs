using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AnnotationMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AnnotationModel, AnnotationInputModel>()
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Text, src => src.Text)
            .Map(dest => dest.Uri, src => src.Uri);

        config.NewConfig<AnnotationModel, Annotation>()
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Id, src => src.Id)
            .Ignore(dest => dest.Position)
            .Map(dest => dest.Text, src => src.Text)
            .Map(dest => dest.Uri, src => src.Uri);

        config.NewConfig<Annotation, AnnotationInputModel>()
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Text, src => src.Text)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Uri, src => src.Uri);
    }
}