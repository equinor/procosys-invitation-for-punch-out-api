namespace Equinor.ProCoSys.Auth.Authentication
{
    public interface IApiAuthenticator
    {
        AuthenticationType AuthenticationType { get; set; }
    }
}
