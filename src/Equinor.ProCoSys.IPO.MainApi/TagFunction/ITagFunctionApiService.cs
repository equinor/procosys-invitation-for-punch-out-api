using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.TagFunction
{
    public interface ITagFunctionApiService
    {
        Task<ProCoSysTagFunction> TryGetTagFunctionAsync(string plant, string tagFunctionCode, string registerCode);
    }
}
