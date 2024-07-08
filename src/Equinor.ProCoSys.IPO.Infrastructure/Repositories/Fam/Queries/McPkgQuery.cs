namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class McPkgQuery
{
    public static string Query => @$"
            SELECT
	            m.Guid,
	            m.Plant,
	            p.Name as ProjectName,
	            m.McPkgGuid,
	            i.Guid as InvitationGuid,
	            m.CreatedAtUtc
            FROM McPkgs m
	            JOIN Projects p on p.Id = m.ProjectId
	            JOIN Invitations i on i.Id = m.InvitationId	
            ";
}
