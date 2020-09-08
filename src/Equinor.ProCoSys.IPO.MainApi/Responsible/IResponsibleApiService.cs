using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Responsible
{
    public interface IResponsibleApiService
    {
        Task<ProCoSysResponsible> TryGetResponsibleAsync(string plant, string code);
    }
}
