using Azure.Core;

namespace Equinor.ProCoSys.IPO.Fam;

public interface IFamCredential
{
    TokenCredential GetToken();
}
