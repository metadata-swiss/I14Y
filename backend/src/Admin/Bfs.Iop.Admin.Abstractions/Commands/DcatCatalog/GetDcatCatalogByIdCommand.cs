using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DcatCatalog;

public class GetDcatCatalogByIdCommand : IRequest<Models.DcatCatalog>
{
    public GetDcatCatalogByIdCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}
