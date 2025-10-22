using Azure.Core;
using Azure.Identity;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class FamDefaultCredential : IFamCredential
{
    private readonly string _clientId;

    public FamDefaultCredential(IConfiguration config)
    {
        var commonLibConfigConfig = config.GetSection("CommonLibConfig");
        _clientId = commonLibConfigConfig.GetValue<string>("ClientId");
    }

    public TokenCredential GetToken()
    {
        return new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            WorkloadIdentityClientId = _clientId
        });
    }
}
