using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class MailDefaultCredential(IConfiguration config)
    : BaseDefaultCredential(config.GetValue<string>("Graph:ClientId")), IMailCredential;
