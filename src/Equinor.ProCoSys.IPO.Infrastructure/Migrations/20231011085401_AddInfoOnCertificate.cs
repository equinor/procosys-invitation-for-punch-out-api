using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInfoOnCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Certificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoided",
                table: "Certificates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled','AttendedStatusUpdated','NoteUpdated','ScopeHandedOver','StatusReset')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Certificates");

            migrationBuilder.DropColumn(
                name: "IsVoided",
                table: "Certificates");

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled','AttendedStatusUpdated','NoteUpdated','ScopeHandedOver')");
        }
    }
}
