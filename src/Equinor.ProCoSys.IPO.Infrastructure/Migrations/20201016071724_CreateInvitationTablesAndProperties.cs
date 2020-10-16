using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class CreateInvitationTablesAndProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Invitations",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Invitations",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Invitations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CommPkgs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(nullable: false),
                    ProjectName = table.Column<string>(nullable: false),
                    CommPkgNo = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommPkgs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommPkgs_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "McPkgs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(nullable: false),
                    ProjectName = table.Column<string>(nullable: false),
                    CommPkgNo = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McPkgs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McPkgs_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(nullable: false),
                    Organization = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    FunctionalRoleCode = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    AzureOid = table.Column<Guid>(nullable: true),
                    SortKey = table.Column<int>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_InvitationId",
                table: "CommPkgs",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_InvitationId",
                table: "McPkgs",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_InvitationId",
                table: "Participants",
                column: "InvitationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommPkgs");

            migrationBuilder.DropTable(
                name: "McPkgs");

            migrationBuilder.DropTable(
                name: "Participants");

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
