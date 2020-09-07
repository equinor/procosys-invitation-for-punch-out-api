using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.MainApi.Client
{
    public interface IBearerTokenProvider
    {
        ValueTask<string> GetBearerTokenOnBehalfOfCurrentUserAsync();
    }
}
