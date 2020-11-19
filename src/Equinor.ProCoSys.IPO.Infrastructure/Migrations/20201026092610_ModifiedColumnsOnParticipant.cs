using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class ModifiedColumnsOnParticipant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAtUtc",
                table: "Participants",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedById",
                table: "Participants",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_ModifiedById",
                table: "Participants",
                column: "ModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Persons_ModifiedById",
                table: "Participants",
                column: "ModifiedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Persons_ModifiedById",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_ModifiedById",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "ModifiedAtUtc",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "ModifiedById",
                table: "Participants");
        }
    }
}
