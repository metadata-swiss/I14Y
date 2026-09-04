using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Bfs.Iop.DataAccess.Relational.EntityTypeConfigurations.Extensions;

internal static class ModelBuilderExtensions
{
    public static void ToSnakeCaseModel(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName()!.ToSnakeCase();
            entity.SetTableName(tableName);

            var schema = entity.GetSchema();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName(storeObjectIdentifier)?.ToSnakeCase());
            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()?.ToSnakeCase());
            foreach (var key in entity.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName()?.ToSnakeCase());
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
        }
    }
}
