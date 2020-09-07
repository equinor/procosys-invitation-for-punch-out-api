using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Responsible
{
    public interface IResponsibleApiService
    {
        Task<ProcosysResponsible> TryGetResponsibleAsync(string plant, string code);
    }
}
