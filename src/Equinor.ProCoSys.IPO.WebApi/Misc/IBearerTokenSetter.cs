namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public interface IBearerTokenSetter
    {
        void SetBearerToken(string token, bool isUserToken = true);
    }
}
