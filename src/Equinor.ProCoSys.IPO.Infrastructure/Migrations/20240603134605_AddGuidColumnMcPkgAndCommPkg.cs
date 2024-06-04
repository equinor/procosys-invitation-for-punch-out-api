using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidColumnMcPkgAndCommPkg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "McPkgs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //Replace default guid with real guid
            migrationBuilder.Sql(
                @"
                    UPDATE McPkgs
                    SET Guid = NEWID()
                    WHERE Guid = '00000000-0000-0000-0000-000000000000'
                    "
            );

            // Remove the default value
            migrationBuilder.AlterColumn<Guid>(
                name: "Guid",
                table: "McPkgs",
                nullable: false
            );

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "CommPkgs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //Replace default guid with real guid
            migrationBuilder.Sql(
                @"
                    UPDATE CommPkgs
                    SET Guid = NEWID()
                    WHERE Guid = '00000000-0000-0000-0000-000000000000'
                    "
            );

            // Remove the default value
            migrationBuilder.AlterColumn<Guid>(
                name: "Guid",
                table: "CommPkgs",
                nullable: false
            );

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_Guid",
                table: "McPkgs",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_Guid",
                table: "CommPkgs",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_McPkgs_Guid",
                table: "McPkgs");

            migrationBuilder.DropIndex(
                name: "IX_CommPkgs_Guid",
                table: "CommPkgs");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "McPkgs");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "CommPkgs");
        }
    }
}
