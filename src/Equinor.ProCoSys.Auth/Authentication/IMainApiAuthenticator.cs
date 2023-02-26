namespace Equinor.ProCoSys.Auth.Authentication
{
    /// <summary>
    /// The interfaces IBearerTokenProvider, IApiAuthenticator are placed here to be be able 
    /// to configure which implementations are mapped to which interface in DI container setup
    /// </summary>
    public interface IMainApiAuthenticator : IBearerTokenProvider, IApiAuthenticator
    {
    }
}
