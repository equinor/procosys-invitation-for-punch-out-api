using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class CreateInvitationRelatedTablesAndColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Plant",
                table: "Invitations",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Invitations",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ProjectName",
                table: "Invitations",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Invitations",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Invitations",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Invitations",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldNullable: true);

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
                    Plant = table.Column<string>(maxLength: 255, nullable: false),
                    ProjectName = table.Column<string>(maxLength: 512, nullable: false),
                    CommPkgNo = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<string>(nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommPkgs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommPkgs_Persons_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CommPkgs_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "McPkgs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(maxLength: 255, nullable: false),
                    ProjectName = table.Column<string>(maxLength: 512, nullable: false),
                    CommPkgNo = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    McPkgNo = table.Column<string>(nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: false),
                    InvitationId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McPkgs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_McPkgs_Persons_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_McPkgs_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Plant = table.Column<string>(maxLength: 255, nullable: false),
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
                    InvitationId = table.Column<int>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participants_Persons_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Participants_Invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalTable: "Invitations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_CreatedById",
                table: "Invitations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ModifiedById",
                table: "Invitations",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_CreatedById",
                table: "CommPkgs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_InvitationId",
                table: "CommPkgs",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_CreatedById",
                table: "McPkgs",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_InvitationId",
                table: "McPkgs",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_CreatedById",
                table: "Participants",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_InvitationId",
                table: "Participants",
                column: "InvitationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Persons_CreatedById",
                table: "Invitations",
                column: "CreatedById",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Persons_ModifiedById",
                table: "Invitations",
                column: "ModifiedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Persons_CreatedById",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Persons_ModifiedById",
                table: "Invitations");

            migrationBuilder.DropTable(
                name: "CommPkgs");

            migrationBuilder.DropTable(
                name: "McPkgs");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_CreatedById",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_ModifiedById",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Invitations");

            migrationBuilder.AlterColumn<string>(
                name: "Plant",
                table: "Invitations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255);
        }
    }
}
