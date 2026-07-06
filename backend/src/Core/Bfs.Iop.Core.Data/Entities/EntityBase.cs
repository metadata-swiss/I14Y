using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Entities;

public abstract class EntityBase
{
    public DateTimeOffset CreatedAt { get; set; }

    public CreationType? CreationType { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset? ModifiedAt { get; set; }
}
