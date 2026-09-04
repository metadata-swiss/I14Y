using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class GetDatasetQualityInformationCommandHandler : 
    IRequestHandler<GetDatasetQualityInformationCommand, DatasetQualityInformationDataModel>
{
    private readonly IDatasetsService _datasetsService;

    public GetDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService;

    public Task<DatasetQualityInformationDataModel> Handle(GetDatasetQualityInformationCommand request, CancellationToken cancellationToken) => 
        _datasetsService.GetDatasetQualityInformation(request.DatasetId, cancellationToken);
}
