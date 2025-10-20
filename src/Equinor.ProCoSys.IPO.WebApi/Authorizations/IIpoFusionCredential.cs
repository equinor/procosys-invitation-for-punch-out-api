using System.Threading.Tasks;
using Azure.Core;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public interface IIpoFusionCredential
{
    Task<TokenCredential> GetCredentialAsync();
}
