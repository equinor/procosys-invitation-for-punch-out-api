using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IHaveGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_History_ObjectGuid_ASC",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "Oid",
                table: "Persons",
                newName: "Guid");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Oid",
                table: "Persons",
                newName: "IX_Persons_Guid");

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Invitations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SourceGuid",
                table: "History",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_History_SourceGuid_ASC",
                table: "History",
                column: "SourceGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_History_SourceGuid_ASC",
                table: "History");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "SourceGuid",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "Persons",
                newName: "Oid");

            migrationBuilder.RenameIndex(
                name: "IX_Persons_Guid",
                table: "Persons",
                newName: "IX_Persons_Oid");

            migrationBuilder.CreateIndex(
                name: "IX_History_ObjectGuid_ASC",
                table: "History",
                column: "ObjectGuid");
        }
    }
}
