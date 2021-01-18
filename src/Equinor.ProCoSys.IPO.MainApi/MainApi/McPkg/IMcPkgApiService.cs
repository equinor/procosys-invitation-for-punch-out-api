using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public interface IMcPkgApiService
    {
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(string plant, string projectName, string commPkgNo);
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(string plant, string projectName, IList<string> mcPkgNos);
        Task SetM01DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
        Task ClearM01DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
        Task SetM02DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
    }
}
