using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface IBearerTokenProvider
    {
        ValueTask<string> GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync();
        ValueTask<string> GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync();
    }
}
