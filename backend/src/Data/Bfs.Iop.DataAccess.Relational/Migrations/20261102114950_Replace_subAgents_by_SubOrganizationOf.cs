using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Replace_subAgents_by_SubOrganizationOf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sub_organization_of_id",
                schema: "data",
                table: "agent",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_sub_organization_of_id",
                schema: "data",
                table: "agent",
                column: "sub_organization_of_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agent_agent_sub_organization_of_id",
                schema: "data",
                table: "agent",
                column: "sub_organization_of_id",
                principalSchema: "data",
                principalTable: "agent",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
                UPDATE data.agent AS a
                SET sub_organization_of_id = r.agent_id
                FROM (
                    SELECT DISTINCT ON (sub_agent_id) sub_agent_id, agent_id
                    FROM data.agent_sub_agent_relation
                    ORDER BY sub_agent_id, agent_id
                ) AS r
                WHERE a.id = r.sub_agent_id;
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agent_agent_sub_organization_of_id",
                schema: "data",
                table: "agent");

            migrationBuilder.DropIndex(
                name: "ix_agent_sub_organization_of_id",
                schema: "data",
                table: "agent");

            migrationBuilder.DropColumn(
                name: "sub_organization_of_id",
                schema: "data",
                table: "agent");
        }
    }
}
