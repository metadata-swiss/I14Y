using Bfs.Iop.Admin.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DcatCatalogRecords;

public class GetDcatCatalogRecordByResourceIdCommand : IRequest<IEnumerable<DcatCatalogRecordInput>>
{
    public GetDcatCatalogRecordByResourceIdCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}