using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.FilterConfigurations;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.FilterConfigurations;

internal sealed class CreateFilterConfigurationCommandHandler : AsyncRequestHandler<CreateFilterConfigurationCommand>
{
    private readonly FilterConfigurationFileStorageService _filterConfigurationFileStorageService;

    public CreateFilterConfigurationCommandHandler(FilterConfigurationFileStorageService filterConfigurationFileStorageService) =>
        _filterConfigurationFileStorageService = filterConfigurationFileStorageService ?? throw new ArgumentNullException(nameof(filterConfigurationFileStorageService));

    protected override Task Handle(CreateFilterConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _filterConfigurationFileStorageService.UploadAsync(request.Id, request.Stream, cancellationToken);
    }
}