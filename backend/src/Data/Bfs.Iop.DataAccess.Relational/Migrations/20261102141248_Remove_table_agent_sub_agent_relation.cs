using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.DataAccess.Relational.Migrations
{
    /// <inheritdoc />
    public partial class Remove_table_agent_sub_agent_relation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_sub_agent_relation",
                schema: "data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_sub_agent_relation",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_sub_agent_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_sub_agent_relation_agent_agent_id",
                        column: x => x.agent_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agent_sub_agent_relation_agent_sub_agent_id",
                        column: x => x.sub_agent_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agent_sub_agent_relation_agent_id_sub_agent_id",
                schema: "data",
                table: "agent_sub_agent_relation",
                columns: new[] { "agent_id", "sub_agent_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_sub_agent_relation_sub_agent_id",
                schema: "data",
                table: "agent_sub_agent_relation",
                column: "sub_agent_id");
        }
    }
}
