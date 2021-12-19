using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AddUnsignEventType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled')");
        }
    }
}
