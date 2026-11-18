using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Add_external_agent_fields_to_qualifier_attribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "agent_id",
                schema: "data",
                table: "qualified_attribution",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "external_agent_identifier",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_agent_name_de",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_agent_name_en",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_agent_name_fr",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_agent_name_it",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_agent_name_rm",
                schema: "data",
                table: "qualified_attribution",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_agent_identifier",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.DropColumn(
                name: "external_agent_name_de",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.DropColumn(
                name: "external_agent_name_en",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.DropColumn(
                name: "external_agent_name_fr",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.DropColumn(
                name: "external_agent_name_it",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.DropColumn(
                name: "external_agent_name_rm",
                schema: "data",
                table: "qualified_attribution");

            migrationBuilder.AlterColumn<Guid>(
                name: "agent_id",
                schema: "data",
                table: "qualified_attribution",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
