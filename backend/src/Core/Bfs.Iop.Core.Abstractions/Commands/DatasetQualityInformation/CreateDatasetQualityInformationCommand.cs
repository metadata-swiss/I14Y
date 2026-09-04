using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record CreateDatasetQualityInformationCommand(
    Guid DatasetId,
    DatasetQualityInformationDataModel  DatasetQualityInformationDataModel) : IRequest
{ }
