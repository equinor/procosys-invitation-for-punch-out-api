using Equinor.ProCoSys.Auth.Authentication;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    /// <summary>
    /// The interface IBearerTokenProvider are placed here and not on class ApiAuthenticator. 
    /// Thisto be be able to configure which implementations are mapped to which interface in DI container setup
    /// </summary>
    public interface ILibraryApiAuthenticator : IBearerTokenProvider
    {
    }
}
