using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;

public interface IFamRepository
{
    Task<IEnumerable<IParticipantEventV1>> GetParticipants();
    Task<IEnumerable<IInvitationEventV1>> GetInvitations();
    Task<IEnumerable<ICommentEventV1>> GetComments();
    Task<IEnumerable<IMcPkgEventV1>> GetMcPkgs();
    Task<IEnumerable<ICommPkgEventV1>> GetCommPkgs();
}
