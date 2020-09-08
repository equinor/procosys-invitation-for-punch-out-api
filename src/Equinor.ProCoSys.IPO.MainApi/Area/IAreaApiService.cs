using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Area
{
    public interface IAreaApiService
    {
        Task<ProCoSysArea> TryGetAreaAsync(string plant, string code);
    }
}
