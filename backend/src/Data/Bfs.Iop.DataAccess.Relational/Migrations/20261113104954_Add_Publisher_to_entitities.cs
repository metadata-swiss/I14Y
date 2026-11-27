using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Add_Publisher_to_entitities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "public_services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "public_services",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "mapping_tables",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "mapping_tables",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "iop_concepts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "iop_concepts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "dcat_catalog",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "dcat_catalog",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "dataset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "dataset",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_identifier",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_de",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_en",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_it",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "data_service",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "publisher_id",
                schema: "data",
                table: "data_service",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_public_services_publisher_id",
                schema: "data",
                table: "public_services",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_mapping_tables_publisher_id",
                schema: "data",
                table: "mapping_tables",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_iop_concepts_publisher_id",
                schema: "data",
                table: "iop_concepts",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_publisher_id",
                schema: "data",
                table: "dcat_catalog",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_publisher_id",
                schema: "data",
                table: "dataset",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_publisher_id",
                schema: "data",
                table: "data_service",
                column: "publisher_id");

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

            var tables = new[] { "public_services", "mapping_tables", "iop_concepts", "dataset", "data_service", "dcat_catalog" };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"UPDATE data.{table} SET publisher_id = tenant_id;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "ix_public_services_publisher_id",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropIndex(
                name: "ix_mapping_tables_publisher_id",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropIndex(
                name: "ix_iop_concepts_publisher_id",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropIndex(
                name: "ix_dcat_catalog_publisher_id",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropIndex(
                name: "ix_dataset_publisher_id",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropIndex(
                name: "ix_data_service_publisher_id",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "public_services");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "mapping_tables");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "iop_concepts");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "dcat_catalog");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "dataset");

            migrationBuilder.DropColumn(
                name: "external_publisher_identifier",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_de",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_en",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_fr",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_it",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "external_publisher_name_rm",
                schema: "data",
                table: "data_service");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                schema: "data",
                table: "data_service");
        }
    }
}
