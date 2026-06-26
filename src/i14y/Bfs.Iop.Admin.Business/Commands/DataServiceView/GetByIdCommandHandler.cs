using AutoMapper;
using Bfs.Iop.Admin.Commands.DataServiceView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DataServiceView;

internal class GetByIdCommandHandler : IRequestHandler<GetByIdCommand, DataService>
{
    private readonly IIopCoreApiClient _apiClient;
    private readonly IMapper _mapper;

    public GetByIdCommandHandler(
        IIopCoreApiClient apiClient,
        IMapper mapper)
    {
        _apiClient = apiClient;
        _mapper = mapper;
    }

    public async Task<DataService> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDataServicesByIdAsync(request.DataServiceId, cancellationToken);

        var dataService = _mapper.Map<DataService>(response.Result);

        if (response.Result.PreviousVersion is not null)
        {
            dataService.PreviousVersion = await TryGetPreviousVersion(response.Result.PreviousVersion.Id, cancellationToken);
        }

        // Fill next versions
        var nextVersionsResponse = await _apiClient.GetDataServicesNextVersionsByIdAndPageAndPageSizeAsync(
            request.DataServiceId,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        dataService.NextVersions = _mapper.Map<IEnumerable<DataServiceVersionSummary>>(nextVersionsResponse.Result);

        // Fill serves datasets
        var servesDatasetsTasks = response.Result.ServesDatasets
            .Select(async x => await TryGetServedDataset(x.Id, cancellationToken));

        dataService.ServesDatasets = (await Task.WhenAll(servesDatasetsTasks))
            .Where(x => x is not null)
            .Select(x => x!);

        return dataService;
    }

    private async Task<IdLabel?> TryGetServedDataset(Guid datasetId, CancellationToken cancellationToken)
    {
        IdLabel? result;

        try
        {
            var response = await _apiClient.GetDatasetsByIdAsync(datasetId, cancellationToken);

            result = _mapper.Map<IdLabel>(response.Result);
        }
        catch (ApiException ex)
        when (ex.StatusCode is 401 or 403)
        {
            result = null;
        }

        return result;
    }

    private async Task<DataServiceVersionSummary?> TryGetPreviousVersion(Guid previousVersionId, CancellationToken cancellationToken)
    {
        DataServiceVersionSummary? previousVersion;

        try
        {
            var response = await _apiClient.GetDataServicesByIdAsync(previousVersionId, cancellationToken);

            previousVersion = _mapper.Map<DataServiceVersionSummary>(response.Result);
        }
        catch (ApiException ex)
        when (ex.StatusCode is 401 or 403)
        {
            previousVersion = null;
        }

        return previousVersion;
    }
}