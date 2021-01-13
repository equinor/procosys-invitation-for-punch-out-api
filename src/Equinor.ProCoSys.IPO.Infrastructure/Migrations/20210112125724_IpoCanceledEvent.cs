using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class IpoCanceledEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Persons",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Persons",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Persons",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddCheckConstraint(
                name: "constraint_history_check_valid_event_type",
                table: "History",
                sql: "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved')");
        }
    }
}
