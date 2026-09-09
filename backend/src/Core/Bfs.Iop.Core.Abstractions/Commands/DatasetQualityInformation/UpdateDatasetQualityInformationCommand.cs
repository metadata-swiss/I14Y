using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record UpdateDatasetQualityInformationCommand(
    Guid DatasetId,
    DatasetQualityInformationDataModel Model) : IRequest
{ }
