using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.FilterConfigurations;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.FilterConfigurations;

internal sealed class DeleteFilterConfigurationCommandHandler : AsyncRequestHandler<DeleteFilterConfigurationCommand>
{
    private readonly FilterConfigurationFileStorageService _filterConfigurationFileStorageService;

    public DeleteFilterConfigurationCommandHandler(FilterConfigurationFileStorageService filterConfigurationFileStorageService) =>
        _filterConfigurationFileStorageService = filterConfigurationFileStorageService ?? throw new ArgumentNullException(nameof(filterConfigurationFileStorageService));

    protected override Task Handle(DeleteFilterConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _filterConfigurationFileStorageService.DeleteAsync(request.Id, cancellationToken);
    }
}
