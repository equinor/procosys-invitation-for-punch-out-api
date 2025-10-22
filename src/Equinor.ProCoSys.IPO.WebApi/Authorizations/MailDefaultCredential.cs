using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class MailDefaultCredential : IMailCredential
{
    private readonly string _graphClientId;

    public MailDefaultCredential(IConfiguration config)
    {
        var graphConfig = config.GetSection("Graph");
        _graphClientId = graphConfig.GetValue<string>("ClientId");
    }

    public TokenCredential GetToken()
    {
        return new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            WorkloadIdentityClientId = _graphClientId
        });
    }
}
