﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg
{
    public interface IMcPkgApiService
    {
        Task<IList<ProCoSysMcPkgOnCommPkg>> GetMcPkgsByCommPkgNoAndProjectNameAsync(string plant, string projectName, string commPkgNo);
        Task<IList<ProCoSysMcPkg>> GetMcPkgsByMcPkgNosAsync(string plant, string projectName, IList<string> mcPkgNos);
        Task<ProCoSysMcPkg> GetMcPkgByIdAsync(string plant, long mcPkgId);
        Task SetM01DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
        Task ClearM01DatesAsync(string plant, int? invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
        Task SetM02DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
        Task ClearM02DatesAsync(string plant, int invitationId, string projectName, IList<string> mcPkgNos, IList<string> commPkgNos);
    }
}
