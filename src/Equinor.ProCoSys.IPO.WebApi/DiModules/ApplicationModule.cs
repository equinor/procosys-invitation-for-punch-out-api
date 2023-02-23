using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Command.EventHandlers;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Email;
using Equinor.ProCoSys.IPO.Email.Settings;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Equinor.ProCoSys.IPO.WebApi.Excel;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;
using Equinor.ProCoSys.PcsServiceBus.Receiver;
using Equinor.ProCoSys.PcsServiceBus.Receiver.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.Auth.Time;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;

namespace Equinor.ProCoSys.IPO.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            TimeService.SetProvider(new SystemTimeProvider());

            services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
            services.Configure<LibraryApiOptions>(configuration.GetSection("LibraryApi"));
            services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            services.Configure<IpoAuthenticatorOptions>(configuration.GetSection("Authenticator"));
            services.Configure<MeetingOptions>(configuration.GetSection("Meetings"));
            services.Configure<EmailOptions>(configuration.GetSection("Email"));

            services.AddDbContext<IPOContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IPOContext");
                options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Hosted services

            //services.AddHostedService<PcsBusReceiver>(br => new PcsBusReceiver(configuration, br, br.GetService<ILogger<PcsBusReceiver>>()));

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
            services.AddScoped<IProjectChecker, ProjectChecker>();
            services.AddScoped<IInvitationHelper, InvitationHelper>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<IPOContext>());
            services.AddScoped<IReadOnlyContext, IPOContext>();
            services.AddScoped<IBusReceiverService, BusReceiverService>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();

            services.AddScoped<IAuthenticatorOptions, AuthenticatorOptions>();
            services.AddScoped<LibraryApiAuthenticator>();
            services.AddScoped<ILibraryApiTokenProvider>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            //services.AddScoped<IBearerTokenProvider>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            //services.AddScoped<IApiAuthenticator>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            services.AddScoped<IBearerTokenSetterForAll, BearerTokenSetterForAll>();
            services.AddScoped<ILibraryApiClient, LibraryApiClient>();
            services.AddScoped<IPlantApiService, MainApiPlantService>();
            services.AddScoped<IProjectApiService, MainApiProjectService>();
            services.AddScoped<IPermissionApiService, MainApiPermissionService>();
            services.AddScoped<IBlobStorage, AzureBlobService>();
            services.AddScoped<ICommPkgApiService, MainApiCommPkgService>();
            services.AddScoped<IMcPkgApiService, MainApiMcPkgService>();
            services.AddScoped<IFunctionalRoleApiService, LibraryApiFunctionalRoleService>();
            services.AddScoped<IPersonApiService, MainApiPersonService>();
            services.AddScoped<IMeApiService, MainApiMeService>();

            services.AddScoped<IInvitationValidator, InvitationValidator>();
            services.AddScoped<IRowVersionValidator, RowVersionValidator>();
            services.AddScoped<ISavedFilterValidator, SavedFilterValidator>();

            services.AddScoped<IExcelConverter, ExcelConverter>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<ICacheManager, CacheManager>();
            services.AddSingleton<IBusReceiverServiceFactory, ScopedBusReceiverServiceFactory>();
            services.AddSingleton<IEmailService, EmailService>();
        }
    }
}
