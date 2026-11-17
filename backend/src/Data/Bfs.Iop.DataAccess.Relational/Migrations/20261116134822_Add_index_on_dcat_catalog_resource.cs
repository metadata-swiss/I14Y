using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Add_index_on_dcat_catalog_resource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_resource_resource_id_resource_type",
                schema: "data",
                table: "dcat_catalog_resource",
                columns: new[] { "resource_id", "resource_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_dcat_catalog_resource_resource_id_resource_type",
                schema: "data",
                table: "dcat_catalog_resource");
        }
    }
}
