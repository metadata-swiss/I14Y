using Bfs.Iop.DataAccess.Abstractions;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed record MappingTableExportModel : MappingTableModel
{
    public IEnumerable<MappingRelationModel> Relations { get; init; } = [];
}
