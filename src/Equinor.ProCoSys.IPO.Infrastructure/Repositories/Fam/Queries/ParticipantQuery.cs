namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class ParticipantQuery
{
    public static string Query => @$"
          SELECT
	        p.Guid,
	        p.Plant,
	        project.Name as ProjectName,
	        CASE
		        WHEN p.Organization = 0 THEN 'Commissioning'
		        WHEN p.Organization = 1 THEN 'ConstructionCompany'
		        WHEN p.Organization = 2 THEN 'Contractor'
		        WHEN p.Organization = 3 THEN 'Operation'
		        WHEN p.Organization = 4 THEN 'TechnicalIntegrity'
		        WHEN p.Organization = 5 THEN 'Supplier'
		        WHEN p.Organization = 6 THEN 'External'
	        END as Organization,
	        CASE 
		        WHEN p.Type = 0 THEN 'Person'
		        WHEN p.Type = 1 THEN 'FunctionalRole'
	        END as Type,
	        p.FunctionalRoleCode,
	        p.AzureOid,
	        p.SortKey,
	        p.CreatedAtUtc,
	        i.Guid as InvitationGuid,
	        p.ModifiedAtUtc,
	        p.Attended,
	        p.Note,
	        p.SignedAtUtc,
	        signedBy.Guid as SignedByOid
        FROM Participants p
	        JOIN Invitations i on i.Id = p.InvitationId
	        JOIN Projects project on project.Id = i.ProjectId
	        LEFT JOIN Persons signedBy on signedBy.Id = p.SignedBy
	        WHERE ((p.FunctionalRoleCode is null and p.UserName is not null) OR (p.FunctionalRoleCode is not null and p.UserName is null) OR (p.SignedAtUtc is not null))
            ";
}
