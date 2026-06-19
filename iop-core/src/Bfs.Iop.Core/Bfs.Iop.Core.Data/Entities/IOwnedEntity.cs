namespace Bfs.Iop.Core.Data.Entities;

/// <summary>
/// Entity that is owned by a Publisher (Agent).
/// </summary>
internal interface IOwnedEntity
{
    Agent Publisher { get; }

    Guid PublisherId { get; }
}
