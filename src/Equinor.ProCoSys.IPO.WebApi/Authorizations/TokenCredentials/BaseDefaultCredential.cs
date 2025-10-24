using Azure.Core;
using Azure.Identity;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public abstract class BaseDefaultCredential(string clientId)
{
    public TokenCredential GetToken()
    {
        return new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            WorkloadIdentityClientId = clientId
        });
    }
}
