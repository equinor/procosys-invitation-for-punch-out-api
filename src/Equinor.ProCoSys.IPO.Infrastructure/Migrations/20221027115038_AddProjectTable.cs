using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AddProjectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invitations_Plant_ProjectName",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_ProjectName",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "SavedFilters");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "McPkgs");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "CommPkgs");

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "SavedFilters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "McPkgs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Invitations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "CommPkgs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Persons_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Persons_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Persons",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SavedFilters_ProjectId",
                table: "SavedFilters",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_ProjectId",
                table: "McPkgs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ProjectId",
                table: "Invitations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_ProjectId",
                table: "CommPkgs",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ModifiedById",
                table: "Projects",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name_ASC",
                table: "Projects",
                column: "Name")
                .Annotation("SqlServer:Include", new[] { "Plant" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Plant_ASC",
                table: "Projects",
                column: "Plant")
                .Annotation("SqlServer:Include", new[] { "Name", "IsClosed", "CreatedAtUtc", "ModifiedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommPkgs_Projects_ProjectId",
                table: "CommPkgs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Projects_ProjectId",
                table: "Invitations",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_McPkgs_Projects_ProjectId",
                table: "McPkgs",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedFilters_Projects_ProjectId",
                table: "SavedFilters",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommPkgs_Projects_ProjectId",
                table: "CommPkgs");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Projects_ProjectId",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_McPkgs_Projects_ProjectId",
                table: "McPkgs");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedFilters_Projects_ProjectId",
                table: "SavedFilters");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_SavedFilters_ProjectId",
                table: "SavedFilters");

            migrationBuilder.DropIndex(
                name: "IX_McPkgs_ProjectId",
                table: "McPkgs");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_ProjectId",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_CommPkgs_ProjectId",
                table: "CommPkgs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SavedFilters");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "McPkgs");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "CommPkgs");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "SavedFilters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "McPkgs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Invitations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "CommPkgs",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                    name: "IX_Invitations_Plant_ProjectName",
                    table: "Invitations",
                    columns: new[] {"Plant", "ProjectName"})
                .Annotation("SqlServer:Include",
                    new[]
                    {
                        "Title", "Description", "Type", "CompletedAtUtc", "AcceptedAtUtc", "StartTimeUtc",
                        "RowVersion", "Status"
                    });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ProjectName",
                table: "Invitations",
                columns: new[] {"ProjectName"});
        }
    }
}
