namespace Equinor.Procosys.CPO.WebApi.Misc
{
    public interface IBearerTokenSetter
    {
        void SetBearerToken(string token, bool isUserToken = true);
    }
}
