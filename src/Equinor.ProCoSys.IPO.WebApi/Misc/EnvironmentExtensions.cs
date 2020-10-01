using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public static class EnvironmentExtensions
    {
        public static bool IsTest(this IHostEnvironment hostEnvironment) => hostEnvironment.EnvironmentName == "Test";
    }
}
