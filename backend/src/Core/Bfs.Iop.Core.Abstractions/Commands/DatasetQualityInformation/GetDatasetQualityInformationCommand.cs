using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record GetDatasetQualityInformationCommand(Guid DatasetId) : IRequest<DatasetQualityInformationDataModel>
{ }
