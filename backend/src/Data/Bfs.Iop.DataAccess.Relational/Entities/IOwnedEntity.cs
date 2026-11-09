namespace Bfs.Iop.DataAccess.Relational.Entities;

/// <summary>
/// Entity that is owned by a Publisher (Agent).
/// </summary>
internal interface IOwnedEntity
{
    Agent Tenant { get; }

    Guid TenantId { get; }
}
