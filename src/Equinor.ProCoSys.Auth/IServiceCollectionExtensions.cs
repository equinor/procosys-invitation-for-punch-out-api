using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Permission;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {
            services.AddScoped<MainApiAuthenticator>();
            services.AddScoped<IMainApiTokenProvider>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IMainApiClient, MainApiClient>();
            services.AddScoped<IPermissionApiService, MainApiPermissionService>();
            services.AddScoped<IPermissionCache, PermissionCache>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ICacheManager, CacheManager>();

            return services;
        }
    }
}
