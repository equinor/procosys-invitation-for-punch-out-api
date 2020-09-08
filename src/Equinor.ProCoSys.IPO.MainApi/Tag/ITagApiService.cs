using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Tag
{
    public interface ITagApiService
    {
        Task<IList<ProCoSysTagDetails>> GetTagDetailsAsync(string plant, string projectName, IList<string> tagNos);
        Task<IList<ProCoSysTagOverview>> SearchTagsByTagNoAsync(string plant, string projectName, string startsWithTagNo);
        Task<IList<ProCoSysPreservedTag>> GetPreservedTagsAsync(string plant, string projectName);
        Task<IList<ProCoSysTagOverview>> SearchTagsByTagFunctionsAsync(string plant, string projectName, IList<string> tagFunctionCodeRegisterCodePairs);
        Task MarkTagsAsMigratedAsync(string plant, IEnumerable<long> tagIds);
    }
}
