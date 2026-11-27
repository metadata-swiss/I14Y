using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Rename_publisher_as_tenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_data_service_agent_publisher_id",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropForeignKey(
                name: "fk_dataset_agent_publisher_id",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropForeignKey(
                name: "fk_dcat_catalog_agent_publisher_id",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropForeignKey(
                name: "fk_iop_concepts_agent_publisher_id",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropForeignKey(
                name: "fk_mapping_tables_agent_publisher_id",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropForeignKey(
                name: "fk_public_services_agent_publisher_id",
                schema: "data",
                table: "public_services");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "public_services",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_public_services_publisher_id",
                schema: "data",
                table: "public_services",
                newName: "ix_public_services_tenant_id");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "mapping_tables",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_mapping_tables_publisher_id",
                schema: "data",
                table: "mapping_tables",
                newName: "ix_mapping_tables_tenant_id");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "iop_concepts",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_iop_concepts_publisher_id",
                schema: "data",
                table: "iop_concepts",
                newName: "ix_iop_concepts_tenant_id");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "dcat_catalog",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_dcat_catalog_publisher_id",
                schema: "data",
                table: "dcat_catalog",
                newName: "ix_dcat_catalog_tenant_id");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "dataset",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_dataset_publisher_id",
                schema: "data",
                table: "dataset",
                newName: "ix_dataset_tenant_id");

            migrationBuilder.RenameColumn(
                name: "publisher_id",
                schema: "data",
                table: "data_service",
                newName: "tenant_id");

            migrationBuilder.RenameIndex(
                name: "ix_data_service_publisher_id",
                schema: "data",
                table: "data_service",
                newName: "ix_data_service_tenant_id");

            migrationBuilder.AddForeignKey(
                name: "fk_data_service_agent_tenant_id",
                schema: "data",
                table: "data_service",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dataset_agent_tenant_id",
                schema: "data",
                table: "dataset",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dcat_catalog_agent_tenant_id",
                schema: "data",
                table: "dcat_catalog",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iop_concepts_agent_tenant_id",
                schema: "data",
                table: "iop_concepts",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_mapping_tables_agent_tenant_id",
                schema: "data",
                table: "mapping_tables",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_public_services_agent_tenant_id",
                schema: "data",
                table: "public_services",
                column: "tenant_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_data_service_agent_tenant_id",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropForeignKey(
                name: "fk_dataset_agent_tenant_id",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropForeignKey(
                name: "fk_dcat_catalog_agent_tenant_id",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropForeignKey(
                name: "fk_iop_concepts_agent_tenant_id",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropForeignKey(
                name: "fk_mapping_tables_agent_tenant_id",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropForeignKey(
                name: "fk_public_services_agent_tenant_id",
                schema: "data",
                table: "public_services");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "public_services",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_public_services_tenant_id",
                schema: "data",
                table: "public_services",
                newName: "ix_public_services_publisher_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "mapping_tables",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_mapping_tables_tenant_id",
                schema: "data",
                table: "mapping_tables",
                newName: "ix_mapping_tables_publisher_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "iop_concepts",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_iop_concepts_tenant_id",
                schema: "data",
                table: "iop_concepts",
                newName: "ix_iop_concepts_publisher_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "dcat_catalog",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_dcat_catalog_tenant_id",
                schema: "data",
                table: "dcat_catalog",
                newName: "ix_dcat_catalog_publisher_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "dataset",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_dataset_tenant_id",
                schema: "data",
                table: "dataset",
                newName: "ix_dataset_publisher_id");

            migrationBuilder.RenameColumn(
                name: "tenant_id",
                schema: "data",
                table: "data_service",
                newName: "publisher_id");

            migrationBuilder.RenameIndex(
                name: "ix_data_service_tenant_id",
                schema: "data",
                table: "data_service",
                newName: "ix_data_service_publisher_id");

            migrationBuilder.AddForeignKey(
                name: "fk_data_service_agent_publisher_id",
                schema: "data",
                table: "data_service",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dataset_agent_publisher_id",
                schema: "data",
                table: "dataset",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dcat_catalog_agent_publisher_id",
                schema: "data",
                table: "dcat_catalog",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_iop_concepts_agent_publisher_id",
                schema: "data",
                table: "iop_concepts",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_mapping_tables_agent_publisher_id",
                schema: "data",
                table: "mapping_tables",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_public_services_agent_publisher_id",
                schema: "data",
                table: "public_services",
                column: "publisher_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id");
        }
    }
}
