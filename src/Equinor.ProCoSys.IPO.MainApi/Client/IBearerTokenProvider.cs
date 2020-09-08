using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.MainApi.Client
{
    public interface IBearerTokenProvider
    {
        ValueTask<string> GetBearerTokenOnBehalfOfCurrentUserAsync();
    }
}
