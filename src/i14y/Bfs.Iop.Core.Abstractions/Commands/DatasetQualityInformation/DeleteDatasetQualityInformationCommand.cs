using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;

public sealed record DeleteDatasetQualityInformationCommand(Guid DatasetId) : IRequest
{ }
