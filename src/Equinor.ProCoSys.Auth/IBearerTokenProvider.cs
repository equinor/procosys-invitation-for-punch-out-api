using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth
{
    public interface IBearerTokenProvider
    {
        ValueTask<string> GetBearerTokenAsync();
    }
}
