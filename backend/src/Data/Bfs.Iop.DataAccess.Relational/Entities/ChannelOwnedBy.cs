namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class ChannelOwnedBy : EntityBase
{
    public Channel Channel { get; set; } = null!;

    public Guid ChannelId { get; set; }

    public Agent OwnedBy { get; set; } = null!;

    public Guid OwnedById { get; set; }
}