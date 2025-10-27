using System;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;

public class FamCertificateCredential(IConfiguration config) :
    BaseCertificateCredential(
    config.GetValue<string>("Fam:ClientId"),
    config.GetValue<string>("Fam:ClientCredentials:0:KeyVaultCertificateName"),
    new Uri(config.GetValue<string>("Fam:ClientCredentials:0:KeyVaultUrl")!),
    config.GetValue<string>("AzureAd:TenantId")),
    IFamCredential, ITokenCredential;
