using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal sealed class DistributionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.Distribution, DcatDistributionInputModel>()
            .Map(
                dest => dest.AccessUrl,
                src => src.AccessUrls.First(),
                src => src.AccessUrls.Any())
            .Map(dest => dest.Availability, src => src.Availability)
            .Map(dest => dest.ByteSize, src => src.ByteSize)
            .Map(dest => dest.Checksum, src => src.Checksum)
            .Map(dest => dest.ConformsTo, src => src.ConformTos)
            .Map(dest => dest.Coverage, src => src.Coverage)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Documentation, src => src.Documentation)
            .Map(
                dest => dest.DownloadUrl,
                src => src.DownloadUrls.First(),
                src => src.DownloadUrls.Any())
            .Map(dest => dest.Format, src => src.Format)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Images, src => src.Image)
            .Map(dest => dest.Issued, src => src.Published)
            .Map(dest => dest.Languages, src =>
                src.Languages.Select(l => new CodeInputModel
                {
                    Code = l
                }))
            .Map(dest => dest.License, src => src.License)
            .Map(dest => dest.MediaType, src => src.MediaType)
            .Map(dest => dest.Modified, src => src.LastUpdated)
            .Map(dest => dest.PackagingFormat, src => src.PackagingFormat)
            .Map(dest => dest.Rights, src => src.Rights)
            .Map(
                dest => dest.SpatialResolution,
                src => decimal.Parse(src.SpatialResolution.First()),
                src => src.SpatialResolution.Any()
                       && IsStringParsableToDecimal(src.SpatialResolution.First()))
            .Map(dest => dest.TemporalResolution, src => src.TemporalResolution)
            .Map(dest => dest.Title, src => src.Title);
    }

    private static bool IsStringParsableToDecimal(string value) => decimal.TryParse(value, out _);

}