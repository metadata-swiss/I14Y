using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DistributionSummaryMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DcatDistributionModel, DistributionSummary>()
            .Map(dest => dest.ByteSize, src => src.ByteSize)
            .Map(dest => dest.Format, src => src.Format)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Published, src => src.Issued)
            .Map(dest => dest.Languages, src => src.Languages.Select(x => x.Code))
            .Map(dest => dest.LastUpdated, src => src.Modified)
            .Map(dest => dest.Title, src => src.Title);
    }
}