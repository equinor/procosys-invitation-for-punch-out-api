using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class FamDefaultCredential(IConfiguration config)
    : BaseDefaultCredential(config.GetValue<string>("CommonLibConfig:ClientId")), IFamCredential;
