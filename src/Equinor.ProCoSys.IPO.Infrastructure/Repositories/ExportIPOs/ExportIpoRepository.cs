using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Dapper;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;


namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;


public class ExportIpoRepository : DapperRepositoryBase, IExportIpoRepository
{
    public ExportIpoRepository(IPOContext context) : base(context) { }

    public async Task<List<Invitation>> GetInvitationsWithIncludesAsync(
        List<int> invitationIds,
        IPlantProvider plantProvider,
        CancellationToken cancellationToken)
    {
        await using (var connection = Context.Database.GetDbConnection())
        {
            await connection.OpenAsync(cancellationToken);
            await using (var transaction = await connection.BeginTransactionAsync())
            {
                transaction.Commit();
                var parameters = new { plant = plantProvider.Plant };
                using (var results = await connection.QueryMultipleAsync("GetInvitations", parameters, commandType: CommandType.StoredProcedure))
                {
                    var invitations = await results.ReadAsync<Invitation>();
                    var invitationsLookup = invitations.ToLookup(p => p.Id, p => p);

                    var participants = await results.ReadAsync<DapperParticipant>();

                    var commPkgs = await results.ReadAsync<DapperCommPkg>();
                    var mcPkgs = await results.ReadAsync<DapperMcPkg>();

                    var participantsByInvitation = participants.OrderByDescending(p => p.Type).ToLookup(p => p.InvitationId, p => p);
                    var commPkgsByInvitation = commPkgs.ToLookup(c => c.InvitationId, c => c);
                    var mcPkgsByInvitation = mcPkgs.ToLookup(m => m.InvitationId, m => m);

                    foreach (var participantByInvitation in participantsByInvitation)
                    {
                        var participantsForThisInvitation = participantsByInvitation[participantByInvitation.Key].AsList();
                        participantsForThisInvitation.ForEach(p =>
                        {
                            invitationsLookup[participantByInvitation.Key].First().AddParticipant(p);
                        });
                    }

                    foreach (var commPkgByInvitation in commPkgsByInvitation)
                    {
                        var commpkgs = commPkgsByInvitation[commPkgByInvitation.Key].AsList();
                        commpkgs.ForEach(c => {
                            invitationsLookup[commPkgByInvitation.Key].First().AddCommPkg(c);
                        });
                    }

                    foreach (var mcPkgByInvitation in mcPkgsByInvitation)
                    {
                        var mcpkgs = mcPkgsByInvitation[mcPkgByInvitation.Key].AsList();
                        mcpkgs.ForEach(m =>
                        {
                            invitationsLookup[mcPkgByInvitation.Key].First().AddMcPkg(m);
                        });
                    }

                    return invitations.ToList();
                }
            }
        }

        return null;
    }
}
