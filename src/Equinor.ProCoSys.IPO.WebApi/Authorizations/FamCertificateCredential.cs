using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class FamCertificateCredential : IFamCredential
{
    private readonly string _clientId;
    private readonly string _keyVaultCertificateName;
    private readonly Uri _keyVaultUri;
    private readonly string _tenantId;

    public FamCertificateCredential(IConfiguration config)
    {
        var commonLibConfig = config.GetSection("CommonLibConfig");
        _clientId = commonLibConfig.GetValue<string>("ClientId");

        var credentialConfig = commonLibConfig.GetSection("ClientCredentials:0");
        _keyVaultCertificateName = credentialConfig.GetValue<string>("KeyVaultCertificateName");

        var keyVaultUrl = credentialConfig.GetValue<string>("KeyVaultUrl");
        _keyVaultUri = new Uri(keyVaultUrl!);

        var azureAdConfig = config.GetSection("AzureAd");
        _tenantId = azureAdConfig.GetValue<string>("TenantId");
    }

    public TokenCredential GetToken()
    {
        var secret = RetrieveSecret();
        var certificate = ParseCertificate(secret);

        return new ClientCertificateCredential(_tenantId, _clientId, certificate);
    }

    private KeyVaultSecret RetrieveSecret()
    {
        var defaultCredential = new DefaultAzureCredential();
        var secretClient = new SecretClient(_keyVaultUri, defaultCredential);
        return secretClient.GetSecret(_keyVaultCertificateName).Value;
    }

    private static X509Certificate2 ParseCertificate(KeyVaultSecret secret)
    {
        var certificateBytes = Convert.FromBase64String(secret.Value);
        return new X509Certificate2(certificateBytes, (string)null, X509KeyStorageFlags.MachineKeySet);
    }
}
