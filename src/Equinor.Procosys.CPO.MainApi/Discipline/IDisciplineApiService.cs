using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Discipline
{
    public interface IDisciplineApiService
    {
        Task<ProcosysDiscipline> TryGetDisciplineAsync(string plant, string code);
    }
}
