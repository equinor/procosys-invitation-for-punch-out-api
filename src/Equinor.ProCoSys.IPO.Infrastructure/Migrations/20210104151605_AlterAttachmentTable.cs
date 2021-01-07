using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class AlterAttachmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql
                ("DELETE FROM ATTACHMENT WHERE InvitationId IS NULL");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Invitations_InvitationId",
                table: "Attachment");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Persons_CreatedById",
                table: "Attachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment");

            migrationBuilder.RenameTable(
                name: "Attachment",
                newName: "Attachments");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_InvitationId",
                table: "Attachments",
                newName: "IX_Attachments_InvitationId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachment_CreatedById",
                table: "Attachments",
                newName: "IX_Attachments_CreatedById");

            migrationBuilder.AlterColumn<int>(
                name: "InvitationId",
                table: "Attachments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Invitations_InvitationId",
                table: "Attachments",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Persons_CreatedById",
                table: "Attachments",
                column: "CreatedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Invitations_InvitationId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Persons_CreatedById",
                table: "Attachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attachments",
                table: "Attachments");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachment");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_InvitationId",
                table: "Attachment",
                newName: "IX_Attachment_InvitationId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_CreatedById",
                table: "Attachment",
                newName: "IX_Attachment_CreatedById");

            migrationBuilder.AlterColumn<int>(
                name: "InvitationId",
                table: "Attachment",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attachment",
                table: "Attachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Invitations_InvitationId",
                table: "Attachment",
                column: "InvitationId",
                principalTable: "Invitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Persons_CreatedById",
                table: "Attachment",
                column: "CreatedById",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}
