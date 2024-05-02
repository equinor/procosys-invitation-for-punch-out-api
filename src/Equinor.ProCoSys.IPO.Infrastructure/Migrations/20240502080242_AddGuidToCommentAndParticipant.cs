using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuidToCommentAndParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Participants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //Replace default guid with real guid
            migrationBuilder.Sql(
                @"
                    UPDATE Participants
                    SET Guid = NEWID()
                    WHERE Guid = '00000000-0000-0000-0000-000000000000'
                    "
            );

            // Remove the default value
            migrationBuilder.AlterColumn<Guid>(
                name: "Guid",
                table: "Participants",
                nullable: false
            );

            migrationBuilder.AddColumn<Guid>(
                name: "Guid",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            //Replace default guid with real guid
            migrationBuilder.Sql(
                @"
                    UPDATE Comments
                    SET Guid = NEWID()
                    WHERE Guid = '00000000-0000-0000-0000-000000000000' 
                    "
            );

            // Remove the default value
            migrationBuilder.AlterColumn<Guid>(
                name: "Guid",
                table: "Comments",
                nullable: false
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Guid",
                table: "Comments");
        }
    }
}
