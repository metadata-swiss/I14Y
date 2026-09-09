using System;
using Bfs.Iop.Admin.Lindas.Abstractions;
using MediatR;

namespace Bfs.Iop.Admin.Commands.Lindas;

public sealed class GetLindasLinkCommand : IRequest<Uri?>
{
    public required LindasLinkType LinkType { get; set; }

    public required LindasResourceType Type { get; set; }

    public required string Identifier { get; set; }

    public string? Version { get; set; }
}