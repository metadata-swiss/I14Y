using MediatR;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DataServiceInput;

public class GetDataServiceDatasetsCommand : IRequest<IEnumerable<Models.Dataset>>
{
    public GetDataServiceDatasetsCommand(Guid dataServiceId)
    {
        DataServiceId = dataServiceId;
    }

    public Guid DataServiceId { get; }
}