using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public class MailDefaultCredential(IConfiguration config)
    : BaseDefaultCredential(config.GetValue<string>("Graph:ClientId")), IMailCredential;
