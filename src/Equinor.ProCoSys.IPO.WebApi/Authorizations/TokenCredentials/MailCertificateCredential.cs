using System;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public class MailCertificateCredential(IConfiguration config) :
    BaseCertificateCredential(
        config.GetValue<string>("Graph:ClientId"),
        config.GetValue<string>("Graph:ClientCredentials:0:KeyVaultCertificateName"),
        new Uri(config.GetValue<string>("Graph:ClientCredentials:0:KeyVaultUrl")!),
        config.GetValue<string>("AzureAd:TenantId")), IMailCredential;
