using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class ColumnsOnCommAndMcPkgs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommPkgNo",
                table: "McPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "McPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "McPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommPkgNo",
                table: "CommPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CommPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "CommPkg",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CommPkg",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommPkgNo",
                table: "McPkg");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "McPkg");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "McPkg");

            migrationBuilder.DropColumn(
                name: "CommPkgNo",
                table: "CommPkg");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CommPkg");

            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "CommPkg");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CommPkg");
        }
    }
}
