using Bfs.Iop.Core.Abstractions.Models;
using Cerise;
using Microsoft.AspNetCore.Http;

namespace Bfs.Iop.Core.Abstractions.Commands.Media;

public sealed record CreateMediaCommand(IFormFile File) : IRequest<MediaInfoModel>
{ }
