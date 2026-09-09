using Bfs.Iop.DataAccess.Relational.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations;

internal abstract class EntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityBase
{
    public abstract void Configure(EntityTypeBuilder<T> builder);
}
