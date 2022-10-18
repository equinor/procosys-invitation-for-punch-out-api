using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class _3_new_indexes_invitation_mcpkg_commpkg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_McPkgs_Plant_InvitationId",
                table: "McPkgs",
                columns: new[] { "Plant", "InvitationId" })
                .Annotation("SqlServer:Include", new[] { "CommPkgNo" });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Plant_ProjectName",
                table: "Invitations",
                columns: new[] { "Plant", "ProjectName" })
                .Annotation("SqlServer:Include", new[] { "Title", "Description", "Type", "CompletedAtUtc", "AcceptedAtUtc", "StartTimeUtc", "RowVersion", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CommPkgs_Plant_InvitationId",
                table: "CommPkgs",
                columns: new[] { "Plant", "InvitationId" })
                .Annotation("SqlServer:Include", new[] { "CommPkgNo" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_McPkgs_Plant_InvitationId",
                table: "McPkgs");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_Plant_ProjectName",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_CommPkgs_Plant_InvitationId",
                table: "CommPkgs");
        }
    }
}
