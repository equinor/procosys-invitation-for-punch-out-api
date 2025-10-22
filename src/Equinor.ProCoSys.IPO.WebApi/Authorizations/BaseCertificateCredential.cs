using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations;

public abstract class BaseCertificateCredential(
    string clientId,
    string keyVaultCertificateName,
    Uri keyVaultUri,
    string tenantId)
{
    public TokenCredential GetToken()
    {
        var secret = RetrieveSecret();
        var certificate = ParseCertificate(secret);

        return new ClientCertificateCredential(tenantId, clientId, certificate);
    }

    private KeyVaultSecret RetrieveSecret()
    {
        var defaultCredential = new DefaultAzureCredential();
        var secretClient = new SecretClient(keyVaultUri, defaultCredential);
        return secretClient.GetSecret(keyVaultCertificateName).Value;
    }

    private static X509Certificate2 ParseCertificate(KeyVaultSecret secret)
    {
        var certificateBytes = Convert.FromBase64String(secret.Value);
        return new X509Certificate2(certificateBytes, (string)null, X509KeyStorageFlags.MachineKeySet);
    }
}
