using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.MessageContracts;
using Equinor.ProCoSys.PcsServiceBus.Queries;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;

public class FamRepository : DapperRepositoryBase, IFamRepository
{
    public FamRepository(IPOContext context) : base(context) { }

    public async Task<IEnumerable<IParticipantEventV1>> GetParticipants()
    {
        var query = GetQuery(null);

        return await QueryAsync<ParticipantEvent>(query.query, query.parameters);
    }

    public static (string query, DynamicParameters parameters) GetQuery(string? plant = null)
    {
        var dynamicParameters = new DynamicParameters();

        var query = @$"
          SELECT
	        p.Guid as ProCoSysGuid,
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
	        signedBy.Guid as SignedByOid,
	        p.InvitationId
        FROM Participants p
	        JOIN Invitations i on i.Id = p.InvitationId
	        JOIN Projects project on project.Id = i.ProjectId
	        LEFT JOIN Persons signedBy on signedBy.Id = p.SignedBy
	        WHERE ((p.FunctionalRoleCode is null and p.UserName is not null) OR (p.FunctionalRoleCode is not null and p.UserName is null) OR (p.SignedAtUtc is not null))
            ";

        return (query, dynamicParameters);
    }
}
