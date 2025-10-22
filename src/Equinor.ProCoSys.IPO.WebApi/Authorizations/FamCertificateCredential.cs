using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class FamCertificateCredential(IConfiguration config) : 
    BaseCertificateCredential(
    config.GetValue<string>("CommonLibConfig:ClientId"),
    config.GetValue<string>("CommonLibConfig:ClientCredentials:0:KeyVaultCertificateName"),
    new Uri(config.GetValue<string>("CommonLibConfig:ClientCredentials:0:KeyVaultUrl")!),
    config.GetValue<string>("AzureAd:TenantId")), 
    IFamCredential;
