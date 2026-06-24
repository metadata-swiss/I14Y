using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record UpdateDatasetQualityInformationCommand(
    Guid DatasetId,
    DatasetQualityInformationDataModel Model) : IRequest
{ }
