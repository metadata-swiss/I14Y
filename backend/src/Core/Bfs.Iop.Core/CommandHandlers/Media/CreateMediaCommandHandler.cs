using Bfs.Iop.Core.Abstractions.Commands.Media;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Media;

internal sealed class CreateMediaCommandHandler : IRequestHandler<CreateMediaCommand, MediaInfoModel>
{
    private readonly IMediaService _mediaService;

    public CreateMediaCommandHandler(IMediaService mediaService) => 
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));

    public Task<MediaInfoModel> Handle(CreateMediaCommand request, CancellationToken cancellationToken) =>
        _mediaService.UploadFile(request.File, cancellationToken);
}
