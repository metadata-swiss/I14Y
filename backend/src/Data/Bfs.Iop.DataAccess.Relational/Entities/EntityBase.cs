using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Relational.Entities;

public abstract class EntityBase
{
    public DateTimeOffset CreatedAt { get; set; }

    public CreationType? CreationType { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }
}
