using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.MessageContracts;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;

public interface IFamRepository
{
    Task<IEnumerable<IParticipantEventV1>> GetParticipants();
}
