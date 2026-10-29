using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Services.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Media;

internal sealed class DeleteMediaCommandHandler : AsyncRequestHandler<DeleteMediaCommand>
{
    private readonly IMediaService _mediaService;

    public DeleteMediaCommandHandler(IMediaService mediaService) =>
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    protected override Task Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
        => _mediaService.DeleteFile(request.Url, cancellationToken);
}
