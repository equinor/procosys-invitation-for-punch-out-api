using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.IPO.WebApi.Extensions;

public static class ConfigurationIsDevOnLocalhost
{
    public static bool IsDevOnLocalhost(this IConfiguration configuration)
        => configuration.GetValue<bool>("Application:DevOnLocalhost");
}
