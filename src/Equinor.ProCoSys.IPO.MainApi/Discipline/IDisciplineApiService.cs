using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Discipline
{
    public interface IDisciplineApiService
    {
        Task<ProCoSysDiscipline> TryGetDisciplineAsync(string plant, string code);
    }
}
