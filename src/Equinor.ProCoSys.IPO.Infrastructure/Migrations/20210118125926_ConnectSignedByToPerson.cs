using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class ConnectSignedByToPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql("UPDATE PARTICIPANTS SET SIGNEDBY = NULL");
            migrationBuilder.Sql("UPDATE PARTICIPANTS SET SIGNEDATUTC = NULL");
            migrationBuilder.Sql("UPDATE INVITATIONS SET STATUS = 0");

            migrationBuilder.AlterColumn<int>(
                name: "SignedBy",
                table: "Participants",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Invitations",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAtUtc",
                table: "Invitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AcceptedBy",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAtUtc",
                table: "Invitations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompletedBy",
                table: "Invitations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SignedBy",
                table: "Participants",
                column: "SignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_AcceptedBy",
                table: "Invitations",
                column: "AcceptedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_CompletedBy",
                table: "Invitations",
                column: "CompletedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Persons_AcceptedBy",
                table: "Invitations",
                column: "AcceptedBy",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Persons_CompletedBy",
                table: "Invitations",
                column: "CompletedBy",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Persons_SignedBy",
                table: "Participants",
                column: "SignedBy",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Persons_AcceptedBy",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Persons_CompletedBy",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Persons_SignedBy",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_SignedBy",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_AcceptedBy",
                table: "Invitations");

            migrationBuilder.DropIndex(
                name: "IX_Invitations_CompletedBy",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "AcceptedAtUtc",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "AcceptedBy",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "CompletedAtUtc",
                table: "Invitations");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "Invitations");

            migrationBuilder.AlterColumn<string>(
                name: "SignedBy",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Invitations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);
        }
    }
}
