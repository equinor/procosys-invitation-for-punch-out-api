using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionDefaultCredential : IIpoFusionCredential
{
    private readonly string _fusionClientId;

    public IpoFusionDefaultCredential(IConfiguration config)
    {
        var meetingConfig = config.GetSection("Meetings");
        _fusionClientId = meetingConfig.GetValue<string>("ClientId");
    }

    public Task<TokenCredential> GetCredentialAsync()
    {
        return Task.FromResult<TokenCredential>(new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            WorkloadIdentityClientId = _fusionClientId
        }));
    }
}
