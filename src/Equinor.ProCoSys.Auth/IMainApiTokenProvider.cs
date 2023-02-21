using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth
{
    public interface IMainApiTokenProvider
    {
        ValueTask<string> GetBearerTokenForMainApiOnBehalfOfCurrentUserAsync();
        ValueTask<string> GetBearerTokenForMainApiForApplicationAsync();
    }
}
