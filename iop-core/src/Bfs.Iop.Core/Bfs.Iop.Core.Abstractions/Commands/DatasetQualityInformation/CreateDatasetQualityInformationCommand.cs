using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record CreateDatasetQualityInformationCommand(
    Guid DatasetId,
    DatasetQualityInformationDataModel  DatasetQualityInformationDataModel) : IRequest
{ }
