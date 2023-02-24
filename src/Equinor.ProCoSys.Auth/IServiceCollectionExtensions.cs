using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Auth.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            services.AddScoped<MainApiAuthenticator>();
            services.AddScoped<IMainApiTokenProvider>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IMainApiClient, MainApiClient>();
            services.AddScoped<IPermissionApiService, MainApiPermissionService>();
            services.AddScoped<IPermissionCache, PermissionCache>();
            services.AddScoped<IClaimsPrincipalProvider, ClaimsPrincipalProvider>();
            services.AddScoped<CurrentUserProvider>();
            services.AddScoped<ICurrentUserProvider>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<ICurrentUserSetter>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<IBearerTokenSetterForAll, BearerTokenSetterForAll>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ICacheManager, CacheManager>();

            return services;
        }
    }
}
