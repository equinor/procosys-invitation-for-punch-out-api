using System;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.Auth
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddPcsAuthIntegration(this IServiceCollection services, Action<AuthenticatorOptions2> options)
        {
            var optionsBuilder = new AuthenticatorOptions2();
            options(optionsBuilder);

            services.AddScoped<MainApiAuthenticator>();
            services.AddScoped<IMainApiTokenProvider>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<MainApiAuthenticator>());
            services.AddScoped<IMainApiClient, MainApiClient>();


            return services;
        }
    }
}
