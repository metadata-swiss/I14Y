using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Media;

internal sealed class GetMediaCommandHandler : IRequestHandler<GetMediaCommand, ExportFile>
{
    private readonly IMediaService _mediaService;

    public GetMediaCommandHandler(IMediaService mediaService) => 
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    public Task<ExportFile> Handle(GetMediaCommand request, CancellationToken cancellationToken) =>
        _mediaService.GetFile(request.Url, cancellationToken);
}
