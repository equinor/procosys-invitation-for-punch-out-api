using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AddColumnsToParticipants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "Participants",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Participants",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAtUtc",
                table: "Participants",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignedBy",
                table: "Participants",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Participants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attended",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "SignedAtUtc",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "SignedBy",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Participants");
        }
    }
}
