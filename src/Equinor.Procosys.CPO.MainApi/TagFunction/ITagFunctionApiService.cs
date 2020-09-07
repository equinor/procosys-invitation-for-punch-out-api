using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.TagFunction
{
    public interface ITagFunctionApiService
    {
        Task<ProcosysTagFunction> TryGetTagFunctionAsync(string plant, string tagFunctionCode, string registerCode);
    }
}
