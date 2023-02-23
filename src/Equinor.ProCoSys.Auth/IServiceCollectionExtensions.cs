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
            //services.AddScoped<IBearerTokenProvider>(x => x.GetRequiredService<MainApiAuthenticator>());
            //services.AddScoped<IApiAuthenticator>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IMainApiClient, MainApiClient>();

            return services;
        }
    }
}
