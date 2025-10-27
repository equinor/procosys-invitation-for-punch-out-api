using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public class FamDefaultCredential(IConfiguration config)
    : BaseDefaultCredential(config.GetValue<string>("Fam:ClientId")), IFamCredential, ITokenCredential;
