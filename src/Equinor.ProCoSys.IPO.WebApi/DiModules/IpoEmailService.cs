using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.DIModules;

public class IpoEmailService(
    IOptionsMonitor<EmailOptions> emailOptions,
    IMailCredential mailCredential,
    ILogger<EmailService> logger)
    : EmailService(emailOptions, mailCredential.GetToken(), logger);
