using Bfs.Iop.DataAccess.Abstractions;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record GetDatasetQualityInformationCommand(Guid DatasetId) : IRequest<DatasetQualityInformationDataModel>
{ }
