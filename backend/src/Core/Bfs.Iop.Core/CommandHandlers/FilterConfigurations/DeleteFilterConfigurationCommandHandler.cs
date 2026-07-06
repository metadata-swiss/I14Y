using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.FilterConfigurations;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.FilterConfigurations;

internal sealed class DeleteFilterConfigurationCommandHandler : IRequestHandler<DeleteFilterConfigurationCommand>
{
    private readonly FilterConfigurationFileStorageService _filterConfigurationFileStorageService;

    public DeleteFilterConfigurationCommandHandler(FilterConfigurationFileStorageService filterConfigurationFileStorageService) =>
        _filterConfigurationFileStorageService = filterConfigurationFileStorageService ?? throw new ArgumentNullException(nameof(filterConfigurationFileStorageService));

    public Task Handle(DeleteFilterConfigurationCommand request, CancellationToken cancellationToken)
    {
        return _filterConfigurationFileStorageService.DeleteAsync(request.Id, cancellationToken);
    }
}
