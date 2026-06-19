using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record GetDatasetQualityInformationCommand(Guid DatasetId) : IRequest<DatasetQualityInformationDataModel>
{ }
