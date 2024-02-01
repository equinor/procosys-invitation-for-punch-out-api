using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureForIPOExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var storedProcedureText = @"
                        CREATE PROCEDURE GetInvitations
                            @plant nvarchar(255)
                        AS
                        BEGIN
                            DECLARE @TableVariable TABLE (Id INT)

                            INSERT INTO @TableVariable
                            SELECT Id FROM Invitations WHERE Plant = @plant

                            SELECT Id, ProjectId, Title, CreatedById, Status, Description, Type, Location, 
                                   StartTimeUtc, EndTimeUtc, CompletedAtUtc, AcceptedAtUtc, CreatedAtUtc
                            FROM Invitations i
                            WHERE EXISTS (SELECT 1 FROM @TableVariable v WHERE v.Id = i.Id) AND i.Plant = @plant

                            SELECT p.InvitationId, p.Id As ParticipantId, p.FirstName, p.LastName, p.Type, p.FunctionalRoleCode, p.SortKey, p.Attended, p.Note, p.SignedBy, p.SignedAtUtc, p.Organization, p.CreatedById
                            FROM Participants p
                            INNER JOIN Invitations i ON i.Id = p.InvitationId AND i.Plant = @plant
                            WHERE EXISTS (SELECT 1 FROM @TableVariable v WHERE v.Id = p.InvitationId)

                            SELECT c.InvitationId, c.Id, c.CommPkgNo, c.Description
                            FROM CommPkgs c
                            INNER JOIN Invitations i ON i.Id = c.InvitationId AND i.Plant = @plant
                            WHERE EXISTS (SELECT 1 FROM @TableVariable v WHERE v.Id = c.InvitationId)

                            SELECT m.InvitationId, m.Id, m.McPkgNo, m.Description
                            FROM McPkgs m
                            INNER JOIN Invitations i ON i.Id = m.InvitationId AND i.Plant = @plant
                            WHERE EXISTS (SELECT 1 FROM @TableVariable v WHERE v.Id = m.InvitationId)
                        END";

            migrationBuilder.Sql(storedProcedureText);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE GetInvitations");
        }
    }
}
