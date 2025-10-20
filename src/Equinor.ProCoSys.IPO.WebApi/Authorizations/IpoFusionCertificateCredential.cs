using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public class IpoFusionCertificateCredential : IIpoFusionCredential
{
    private readonly string _clientId;
    private readonly string _keyVaultCertificateName;
    private readonly Uri _keyVaultUri;
    private readonly string _tenantId;

    public IpoFusionCertificateCredential(IConfiguration config)
    {
        var fusionConfig = config.GetSection("Fusion");
        _clientId = fusionConfig.GetValue<string>("ClientId");

        var credentialConfig = fusionConfig.GetSection("ClientCredentials:0");
        _keyVaultCertificateName = credentialConfig.GetValue<string>("KeyVaultCertificateName");

        var keyVaultUrl = credentialConfig.GetValue<string>("KeyVaultUrl");
        _keyVaultUri = new Uri(keyVaultUrl!);

        var azureAdConfig = config.GetSection("AzureAd");
        _tenantId = azureAdConfig.GetValue<string>("TenantId");
    }

    public async Task<TokenCredential> GetCredentialAsync()
    {
        var secret = await RetrieveSecretAsync();
        var certificate = ParseCertificate(secret);

        return new ClientCertificateCredential(_tenantId, _clientId, certificate);
    }

    private async Task<KeyVaultSecret> RetrieveSecretAsync()
    {
        var defaultCredential = new DefaultAzureCredential();
        var secretClient = new SecretClient(_keyVaultUri, defaultCredential);
        var secret  = await secretClient.GetSecretAsync(_keyVaultCertificateName);

        return secret.Value;
    }

    private static X509Certificate2 ParseCertificate(KeyVaultSecret secret)
    {
        var certificateBytes = Convert.FromBase64String(secret.Value);
        return new X509Certificate2(certificateBytes, (string)null, X509KeyStorageFlags.MachineKeySet);
    }
}
