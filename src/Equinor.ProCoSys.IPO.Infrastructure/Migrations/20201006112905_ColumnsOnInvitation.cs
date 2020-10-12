using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class ColumnsOnInvitation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Invitations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Invitations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Invitations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommPkg",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommPkg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommPkg_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McPkg",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McPkg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McPkg_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommPkg_InvitationId",
                table: "CommPkg",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_McPkg_InvitationId",
                table: "McPkg",
                column: "InvitationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommPkg");

            migrationBuilder.DropTable(
                name: "McPkg");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Invitations");
        }
    }
}
