using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.FilterConfigurations;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.FilterConfigurations;

internal sealed class GetFilterConfigurationCommandHandler : IRequestHandler<GetFilterConfigurationCommand, FilterConfigurationModel>
{
    private readonly FilterConfigurationFileStorageService _filterConfigurationFileStorageService;

    public GetFilterConfigurationCommandHandler(FilterConfigurationFileStorageService filterConfigurationFileStorageService) =>
        _filterConfigurationFileStorageService = filterConfigurationFileStorageService ?? throw new ArgumentNullException(nameof(filterConfigurationFileStorageService));

    public Task<FilterConfigurationModel> Handle(GetFilterConfigurationCommand request, CancellationToken cancellationToken)
        => _filterConfigurationFileStorageService.DownloadAsync(request.Id, cancellationToken);
}