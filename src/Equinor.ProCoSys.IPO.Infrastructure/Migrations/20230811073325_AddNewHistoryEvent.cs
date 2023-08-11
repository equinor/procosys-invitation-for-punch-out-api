using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewHistoryEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled','AttendedStatusUpdated','NoteUpdated','ScopeHandedOver')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled','AttendedStatusUpdated','NoteUpdated')");
        }
    }
}
