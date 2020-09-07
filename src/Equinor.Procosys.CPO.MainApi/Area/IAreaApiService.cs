using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Area
{
    public interface IAreaApiService
    {
        Task<ProcosysArea> TryGetAreaAsync(string plant, string code);
    }
}
