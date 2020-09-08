using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public interface IApplicationAuthenticator
    {
        ValueTask<string> GetBearerTokenForApplicationAsync();
    }
}
