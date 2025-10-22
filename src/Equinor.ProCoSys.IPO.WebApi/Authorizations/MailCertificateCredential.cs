using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class MailCertificateCredential(IConfiguration config) :
    BaseCertificateCredential(
        config.GetValue<string>("Graph:ClientId"),
        config.GetValue<string>("Graph:ClientCredentials:0:KeyVaultCertificateName"),
        new Uri(config.GetValue<string>("Graph:ClientCredentials:0:KeyVaultUrl")!),
        config.GetValue<string>("AzureAd:TenantId")), IMailCredential;
