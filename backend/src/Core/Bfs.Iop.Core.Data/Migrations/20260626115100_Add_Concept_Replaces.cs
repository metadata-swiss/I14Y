using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Concept_Replaces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "iop_concept_replaces_id",
                schema: "data",
                table: "resource",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_iop_concept_replaces_id",
                schema: "data",
                table: "resource",
                column: "iop_concept_replaces_id");

            migrationBuilder.AddForeignKey(
                name: "fk_resource_iop_concepts_iop_concept_replaces_id",
                schema: "data",
                table: "resource",
                column: "iop_concept_replaces_id",
                principalSchema: "data",
                principalTable: "iop_concepts",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_resource_iop_concepts_iop_concept_replaces_id",
                schema: "data",
                table: "resource");

            migrationBuilder.DropIndex(
                name: "ix_resource_iop_concept_replaces_id",
                schema: "data",
                table: "resource");

            migrationBuilder.DropColumn(
                name: "iop_concept_replaces_id",
                schema: "data",
                table: "resource");
        }
    }
}
