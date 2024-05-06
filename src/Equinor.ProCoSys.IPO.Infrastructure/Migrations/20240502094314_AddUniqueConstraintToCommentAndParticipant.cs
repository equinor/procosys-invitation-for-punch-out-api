using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToCommentAndParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Participants_Guid",
                table: "Participants",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_Guid",
                table: "Comments",
                column: "Guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_Guid",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Comments_Guid",
                table: "Comments");
        }
    }
}
