using Azure.Core;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public class IpoTokenCredential(TokenCredential credential) : ITokenCredential
{
    public TokenCredential GetToken() => credential;
}
