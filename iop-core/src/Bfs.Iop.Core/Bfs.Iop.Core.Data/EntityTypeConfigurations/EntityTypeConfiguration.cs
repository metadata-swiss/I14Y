using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bfs.Iop.Core.Data.EntityTypeConfigurations;

internal abstract class EntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : EntityBase
{
    public abstract void Configure(EntityTypeBuilder<T> builder);
}
