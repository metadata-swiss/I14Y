using System;
using Bfs.Iop.Admin.Models.Lindas;
using MediatR;

namespace Bfs.Iop.Admin.Commands.Lindas;

public sealed class GetLindasRdfLinkCommand : IRequest<Uri?>
{
    public required LindasResourceType Type { get; set; }

    public required string Identifier { get; set; }

    public string? Version { get; set; }
}