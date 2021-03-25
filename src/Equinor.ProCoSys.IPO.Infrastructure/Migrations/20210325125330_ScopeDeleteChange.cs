using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class ScopeDeleteChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommPkgs_Invitations_InvitationId",
                table: "CommPkgs");

            migrationBuilder.DropForeignKey(
                name: "FK_McPkgs_Invitations_InvitationId",
                table: "McPkgs");

            migrationBuilder.AddForeignKey(
                name: "FK_CommPkgs_Invitations_InvitationId",
                table: "CommPkgs",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_McPkgs_Invitations_InvitationId",
                table: "McPkgs",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommPkgs_Invitations_InvitationId",
                table: "CommPkgs");

            migrationBuilder.DropForeignKey(
                name: "FK_McPkgs_Invitations_InvitationId",
                table: "McPkgs");

            migrationBuilder.AddForeignKey(
                name: "FK_CommPkgs_Invitations_InvitationId",
                table: "CommPkgs",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_McPkgs_Invitations_InvitationId",
                table: "McPkgs",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id");
        }
    }
}
