using Bfs.Iop.Admin.Commands.DatasetView;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetView;

internal class GetByIdCommandHandler : IRequestHandler<GetByIdCommand, Dataset>
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

    public async Task<Dataset> Handle(GetByIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _apiClient.GetDatasetsByIdAsync(request.DatasetId, cancellationToken);

        var dataset = _mapper.Map<Dataset>(response.Result);

        if (response.Result.PreviousVersion is not null)
        {
            dataset.PreviousVersion = await TryGetPreviousVersion(response.Result.PreviousVersion.Id, cancellationToken);
        }

        // next version comes here
        var nextVersionsResponse = await _apiClient.GetDatasetsNextVersionsByIdAndPageAndPageSizeAsync(
            request.DatasetId,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        dataset.NextVersions = _mapper.Map<IEnumerable<DatasetVersionSummary>>(nextVersionsResponse.Result);

        return dataset;
    }

    public async Task<DatasetVersionSummary?> TryGetPreviousVersion(Guid previousVersionId, CancellationToken cancellationToken)
    {
        DatasetVersionSummary? previousVersion;

        try
        {
            var response = await _apiClient.GetDatasetsByIdAsync(previousVersionId, cancellationToken);

            previousVersion = _mapper.Map<DatasetVersionSummary>(response.Result);
        }
        catch (ApiException ex)
        when(ex.StatusCode is 401 or 403)
        {
            previousVersion = null;       
        }

        return previousVersion;
    }
}