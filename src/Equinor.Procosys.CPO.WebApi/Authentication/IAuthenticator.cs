using System.Threading.Tasks;

namespace Equinor.Procosys.CPO.WebApi.Authentication
{
    public interface IApplicationAuthenticator
    {
        ValueTask<string> GetBearerTokenForApplicationAsync();
    }
}
