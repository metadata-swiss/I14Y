namespace Bfs.Iop.DataAccess.Relational.Entities;

/// <summary>
/// Main entity, that may live on its own.
/// Entities that implement this interface won't be automatically deleted by <see cref="IopDbContext.FindAndDeleteOrphanEntities"/> 
/// when all foreign keys are set to null.
/// </summary>
internal interface IMainEntity
{ }
