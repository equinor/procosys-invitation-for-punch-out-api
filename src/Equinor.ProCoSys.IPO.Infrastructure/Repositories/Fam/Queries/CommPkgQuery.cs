namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class CommPkgQuery
{
    public static string Query => @$"
            SELECT
	            c.Guid as ProCoSysGuid,
	            c.Plant,
	            p.Name as ProjectName,
	            i.Guid as InvitationGuid,
	            c.CreatedAtUtc,
	            c.CommPkgGuid
            FROM CommPkgs c
	            JOIN Projects p on p.Id = c.ProjectId
	            JOIN Invitations i on i.Id = c.InvitationId	
            ";
}
