using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Equinor.ProCoSys.IPO.Command.Events;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;

public class FamRepository : DapperRepositoryBase, IFamRepository
{
    public FamRepository(IPOContext context) : base(context) { }

    public async Task<IEnumerable<IParticipantEventV1>> GetParticipants() 
        => await QueryAsync<ParticipantEvent>(ParticipantQuery.Query, new DynamicParameters());

    public async Task<IEnumerable<IInvitationEventV1>> GetInvitations()
        => await QueryAsync<InvitationEvent>(InvitationQuery.Query, new DynamicParameters());

    public async Task<IEnumerable<ICommentEventV1>> GetComments()
        => await QueryAsync<CommentEvent>(CommentQuery.Query, new DynamicParameters());

    public async Task<IEnumerable<IMcPkgEventV1>> GetMcPkgs()
        => await QueryAsync<McPkgEvent>(McPkgQuery.Query, new DynamicParameters());

    public async Task<IEnumerable<ICommPkgEventV1>> GetCommPkgs()
        => await QueryAsync<CommPkgEvent>(CommPkgQuery.Query, new DynamicParameters());
}
