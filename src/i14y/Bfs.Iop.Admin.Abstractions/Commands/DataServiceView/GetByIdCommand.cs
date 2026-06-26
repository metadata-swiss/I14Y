using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DataServiceView;

public class GetByIdCommand : IRequest<Models.DataService>
{
    public GetByIdCommand(Guid dataServiceId)
        => DataServiceId = dataServiceId;

    public Guid DataServiceId { get; }
}