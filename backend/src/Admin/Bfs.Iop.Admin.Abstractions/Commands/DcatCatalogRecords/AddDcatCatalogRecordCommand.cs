using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.DcatCatalogRecords;

public class AddDcatCatalogRecordCommand : IRequest<DcatCatalogRecordInput>
{
    public AddDcatCatalogRecordCommand(DcatCatalogRecordInput model)
    {
        Model = model;
    }

    public DcatCatalogRecordInput Model { get; set; }
}