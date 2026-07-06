using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.PublicServiceView;

public class GetByIdCommand : IRequest<Models.PublicServiceView>
{
    public GetByIdCommand(Guid publicServiceId)
        => Id = publicServiceId;

    public Guid Id { get; }
}