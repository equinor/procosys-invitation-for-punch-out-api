namespace Equinor.ProCoSys.Auth
{
    public interface IApiAuthenticator
    {
        AuthenticationType AuthenticationType { get; set; }
    }
}
