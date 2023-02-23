using Equinor.ProCoSys.Auth;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface ILibraryApiTokenProvider : IBearerTokenProvider, IApiAuthenticator
    {
    }
}
