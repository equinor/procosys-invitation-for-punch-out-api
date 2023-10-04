using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CertificateModifiable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAtUtc",
                table: "Certificates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedById",
                table: "Certificates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ModifiedById",
                table: "Certificates",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_Persons_ModifiedById",
                table: "Certificates",
                column: "ModifiedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_Persons_ModifiedById",
                table: "Certificates");

            migrationBuilder.DropIndex(
                name: "IX_Certificates_ModifiedById",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ModifiedAtUtc",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Certificates");
        }
    }
}
