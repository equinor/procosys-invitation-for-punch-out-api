using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGuidColumnMcPkgAndCommPkg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "McPkgs",
                newName: "McPkgGuid");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "CommPkgs",
                newName: "CommPkgGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "McPkgGuid",
                table: "McPkgs",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "CommPkgGuid",
                table: "CommPkgs",
                newName: "Guid");
        }
    }
}
