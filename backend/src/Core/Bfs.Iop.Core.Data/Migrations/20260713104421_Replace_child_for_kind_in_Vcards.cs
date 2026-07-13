using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Replace_child_for_kind_in_Vcards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "child",
                schema: "data",
                table: "vcard");

            migrationBuilder.AddColumn<int>(
                name: "kind",
                schema: "data",
                table: "vcard",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            var organizationValue = (int)VCardKind.Organization;
            migrationBuilder.Sql($"UPDATE data.vcard SET kind = {organizationValue}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kind",
                schema: "data",
                table: "vcard");

            migrationBuilder.AddColumn<string>(
                name: "child",
                schema: "data",
                table: "vcard",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
