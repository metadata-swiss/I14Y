using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Media;

internal sealed class GetMediaInfosCommandHandler : IRequestHandler<GetMediaInfosCommand, IEnumerable<MediaInfoModel>>
{
    private readonly IMediaService _mediaService;

    public GetMediaInfosCommandHandler(IMediaService mediaService) => 
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    public Task<IEnumerable<MediaInfoModel>> Handle(GetMediaInfosCommand request, CancellationToken cancellationToken) =>
        _mediaService.GetFileInfos(cancellationToken);
}
