using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Media;

internal sealed class DeleteMediaCommandHandler : IRequestHandler<DeleteMediaCommand>
{
    private readonly IMediaService _mediaService;

    public DeleteMediaCommandHandler(IMediaService mediaService) =>
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    public Task Handle(DeleteMediaCommand request, CancellationToken cancellationToken)
        => _mediaService.DeleteFile(request.Url, cancellationToken);
}
