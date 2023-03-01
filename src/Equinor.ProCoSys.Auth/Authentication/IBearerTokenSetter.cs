namespace Equinor.ProCoSys.Auth.Authentication
{
    public interface IBearerTokenSetter
    {
        void SetBearerToken(string bearerToken);
    }
}
