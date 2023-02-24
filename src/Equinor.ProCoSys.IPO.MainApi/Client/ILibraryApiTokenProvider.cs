using Equinor.ProCoSys.Auth.Authentication;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface ILibraryApiTokenProvider : IBearerTokenProvider, IApiAuthenticator
    {
    }
}
