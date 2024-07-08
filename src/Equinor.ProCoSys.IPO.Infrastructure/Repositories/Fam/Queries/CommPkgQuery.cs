namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class CommPkgQuery
{
    public static string Query => @$"
            SELECT
	            c.Guid,
	            c.Plant,
	            p.Name as ProjectName,
	            c.CommPkgGuid,
	            i.Guid as InvitationGuid,
	            c.CreatedAtUtc
            FROM CommPkgs c
	            JOIN Projects p on p.Id = c.ProjectId
	            JOIN Invitations i on i.Id = c.InvitationId	
            ";
}
