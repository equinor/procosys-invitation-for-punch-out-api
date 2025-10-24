using Azure.Core;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public interface ITokenCredential
{
    TokenCredential GetToken();
}
