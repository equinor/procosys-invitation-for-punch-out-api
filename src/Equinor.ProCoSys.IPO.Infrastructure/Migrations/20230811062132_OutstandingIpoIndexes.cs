using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OutstandingIpoIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_InvitationId_Plant",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_Plant",
                table: "Invitations");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Plant_IsClosed",
                table: "Projects",
                columns: new[] { "Plant", "IsClosed" });

            migrationBuilder.CreateIndex(
                name: "IX_Participants_InvitationId_Plant_AzureOid",
                table: "Participants",
                columns: new[] { "InvitationId", "Plant", "AzureOid" })
                .Annotation("SqlServer:Include", new[] { "FunctionalRoleCode", "Organization", "SignedAtUtc", "SortKey", "Type", "SignedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Plant_ProjectId_Status",
                table: "Invitations",
                columns: new[] { "Plant", "ProjectId", "Status" },
                filter: "[Status] <> 3 AND [Status] <> 4")
                .Annotation("SqlServer:Include", new[] { "Description" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_Plant_IsClosed",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Participants_InvitationId_Plant_AzureOid",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_Plant_ProjectId_Status",
                table: "Invitations");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_InvitationId_Plant",
                table: "Participants",
                columns: new[] { "InvitationId", "Plant" })
                .Annotation("SqlServer:Include", new[] { "AzureOid", "FunctionalRoleCode", "Organization", "SignedAtUtc", "SortKey", "Type", "SignedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_Plant",
                table: "Invitations",
                column: "Plant",
                filter: "[Status] <> 3")
                .Annotation("SqlServer:Include", new[] { "Description", "Status" });
        }
    }
}
