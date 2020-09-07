using Equinor.Procosys.CPO.BlobStorage;
using Equinor.Procosys.CPO.Command.EventHandlers;
using Equinor.Procosys.CPO.Command.Validators;
using Equinor.Procosys.CPO.Domain;
using Equinor.Procosys.CPO.Domain.AggregateModels.PersonAggregate;
using Equinor.Procosys.CPO.Domain.Events;
using Equinor.Procosys.CPO.Domain.Time;
using Equinor.Procosys.CPO.Infrastructure;
using Equinor.Procosys.CPO.Infrastructure.Caching;
using Equinor.Procosys.CPO.Infrastructure.Repositories;
using Equinor.Procosys.CPO.MainApi;
using Equinor.Procosys.CPO.MainApi.Area;
using Equinor.Procosys.CPO.MainApi.Certificate;
using Equinor.Procosys.CPO.MainApi.Client;
using Equinor.Procosys.CPO.MainApi.Discipline;
using Equinor.Procosys.CPO.MainApi.Permission;
using Equinor.Procosys.CPO.MainApi.Plant;
using Equinor.Procosys.CPO.MainApi.Project;
using Equinor.Procosys.CPO.MainApi.Responsible;
using Equinor.Procosys.CPO.MainApi.Tag;
using Equinor.Procosys.CPO.MainApi.TagFunction;
using Equinor.Procosys.CPO.WebApi.Authentication;
using Equinor.Procosys.CPO.WebApi.Authorizations;
using Equinor.Procosys.CPO.WebApi.Caches;
using Equinor.Procosys.CPO.WebApi.Misc;
using Equinor.Procosys.CPO.WebApi.Synchronization;
using Equinor.Procosys.CPO.WebApi.Telemetry;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.Procosys.CPO.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
            services.Configure<TagOptions>(configuration.GetSection("ApiOptions"));
            services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            services.Configure<AttachmentOptions>(configuration.GetSection("AttachmentOptions"));
            services.Configure<SynchronizationOptions>(configuration.GetSection("Synchronization"));
            services.Configure<AuthenticatorOptions>(configuration.GetSection("Authenticator"));

            services.AddDbContext<CPOContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CPOContext"));
            });

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Hosted services
            services.AddHostedService<TimedSynchronization>();

            // Transient - Created each time it is requested from the service container


            // Scoped - Created once per client request (connection)
            services.AddScoped<ITelemetryClient, ApplicationInsightsTelemetryClient>();
            services.AddScoped<IPlantCache, PlantCache>();
            services.AddScoped<IPermissionCache, PermissionCache>();
            services.AddScoped<IClaimsTransformation, ClaimsTransformation>();
            services.AddScoped<IClaimsProvider, ClaimsProvider>();
            services.AddScoped<CurrentUserProvider>();
            services.AddScoped<ICurrentUserProvider>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<ICurrentUserSetter>(x => x.GetRequiredService<CurrentUserProvider>());
            services.AddScoped<PlantProvider>();
            services.AddScoped<IPlantProvider>(x => x.GetRequiredService<PlantProvider>());
            services.AddScoped<IPlantSetter>(x => x.GetRequiredService<PlantProvider>());
            services.AddScoped<IAccessValidator, AccessValidator>();
            services.AddScoped<IProjectAccessChecker, ProjectAccessChecker>();
            services.AddScoped<IContentRestrictionsChecker, ContentRestrictionsChecker>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<CPOContext>());
            services.AddScoped<IReadOnlyContext, CPOContext>();
            services.AddScoped<ISynchronizationService, SynchronizationService>();

            services.AddScoped<IPersonRepository, PersonRepository>();

            services.AddScoped<Authenticator>();
            services.AddScoped<IBearerTokenProvider>(x => x.GetRequiredService<Authenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<Authenticator>());
            services.AddScoped<IApplicationAuthenticator>(x => x.GetRequiredService<Authenticator>());
            services.AddScoped<IBearerTokenApiClient, BearerTokenApiClient>();
            services.AddScoped<ITagApiService, MainApiTagService>();
            services.AddScoped<IPlantApiService, MainApiPlantService>();
            services.AddScoped<IProjectApiService, MainApiProjectService>();
            services.AddScoped<IAreaApiService, MainApiAreaService>();
            services.AddScoped<IDisciplineApiService, MainApiDisciplineService>();
            services.AddScoped<IResponsibleApiService, MainApiResponsibleService>();
            services.AddScoped<ITagFunctionApiService, MainApiTagFunctionService>();
            services.AddScoped<IPermissionApiService, MainApiPermissionService>();
            services.AddScoped<ICertificateApiService, MainApiCertificateService>();
            services.AddScoped<IBlobStorage, AzureBlobService>();

            services.AddScoped<IRowVersionValidator, RowVersionValidator>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ICacheManager, CacheManager>();
        }
    }
}
