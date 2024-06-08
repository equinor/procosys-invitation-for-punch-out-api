namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class McPkgQuery
{
    public static string Query => @$"
            SELECT
	            m.Guid as ProCoSysGuid,
	            m.Plant,
	            p.Name as ProjectName,
	            i.Guid as InvitationGuid,
	            m.CreatedAtUtc,
	            m.McPkgGuid
            FROM McPkgs m
	            JOIN Projects p on p.Id = m.ProjectId
	            JOIN Invitations i on i.Id = m.InvitationId	
            ";
}
