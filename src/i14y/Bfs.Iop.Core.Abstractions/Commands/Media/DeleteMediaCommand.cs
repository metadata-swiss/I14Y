using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Media;

public sealed record DeleteMediaCommand(string Url) : IRequest
{ }
