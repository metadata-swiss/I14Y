using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Media;

public sealed record GetMediaInfosCommand() : IRequest<IEnumerable<MediaInfoModel>>
{ }
