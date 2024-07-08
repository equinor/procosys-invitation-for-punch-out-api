namespace Equinor.ProCoSys.IPO.Fam;

public class CommonLibConfig
{
    public CommonLibConfig()
    {
    }

    public CommonLibConfig(string? tenantId, string? clientId, string? clientSecret)
    {
        TenantId = tenantId;
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}
