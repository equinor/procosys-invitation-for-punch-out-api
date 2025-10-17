using Azure.Core;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public interface IMailCredential
{
    TokenCredential GetToken();
}
