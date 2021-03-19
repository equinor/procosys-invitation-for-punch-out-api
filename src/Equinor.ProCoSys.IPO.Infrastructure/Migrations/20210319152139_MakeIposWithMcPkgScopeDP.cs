using Microsoft.EntityFrameworkCore.Migrations;

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    public partial class MakeIposWithMcPkgScopeDP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE INVITATIONS SET TYPE = 0 FROM INVITATIONS JOIN MCPKGS MC ON MC.InvitationId = INVITATIONS.Id WHERE TYPE = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
