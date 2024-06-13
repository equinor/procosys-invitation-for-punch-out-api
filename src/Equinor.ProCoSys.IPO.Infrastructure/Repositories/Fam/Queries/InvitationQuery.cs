namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class InvitationQuery
{
    public static string Query => @$"
            SELECT 
                i.Guid as ProCoSysGuid,
	            i.Plant,
	            p.Name as ProjectName,
	            i.Id,
	            i.CreatedAtUtc,
	            createdBy.Guid as CreatedByOid,
	            i.ModifiedAtUtc,
	            i.Title,
	            CASE
		            WHEN i.Type = 0 THEN 'DP'
		            WHEN i.Type = 1 THEN 'MDP'
	            END AS Type,
	            i.Description,
	            CASE
		            WHEN i.Status = 0 THEN 'Planned'
		            WHEN i.Status = 1 THEN 'Completed'
		            WHEN i.Status = 2 THEN 'Accepted'
		            WHEN i.Status = 3 THEN 'Canceled'
		            WHEN i.Status = 4 THEN 'ScopeHandedOver'
	            END as Status,
	            i.Location,
	            i.StartTimeUtc,
	            i.AcceptedAtUtc,
	            acceptedBy.Guid as AcceptedByOid,
	            i.CompletedAtUtc,
	            completedBy.Guid
              FROM Invitations i
              JOIN Projects p on p.Id = i.ProjectId
              JOIN Persons createdBy on createdBy.Id = i.CreatedById
              LEFT JOIN Persons acceptedBy on acceptedBy.Id = i.AcceptedBy
              LEFT JOIN Persons completedBy on completedBy.Id = i.CompletedBy
                ";
}
