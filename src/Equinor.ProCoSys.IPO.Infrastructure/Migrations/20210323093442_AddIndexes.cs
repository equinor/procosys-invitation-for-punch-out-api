using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AddIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invitations_ProjectName",
                table: "Invitations",
                column: "ProjectName");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Title",
                table: "Invitations",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Status",
                table: "Invitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_CommPkgNo",
                table: "CommPkgs",
                column: "CommPkgNo");

            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_McPkgNo",
                table: "McPkgs",
                column: "McPkgNo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
