using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AddIndexForInvitationAndParticipant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_InvitationId_Plant",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_Plant",
                table: "Invitations");       
        }
    }
}
