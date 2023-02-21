using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface ILibraryApiTokenProvider
    {
        ValueTask<string> GetBearerTokenForLibraryApiOnBehalfOfCurrentUserAsync();
    }
}
