using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.DcatCatalogRecords;

public class UpdateDcatCatalogRecordCommand : IRequest
{
    public UpdateDcatCatalogRecordCommand(DcatCatalogRecordInput model)
    {
        Model = model;
    }

    public DcatCatalogRecordInput Model { get; set; }
}