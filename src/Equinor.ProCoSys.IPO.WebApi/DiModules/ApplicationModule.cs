using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Client;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Caches;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Command.EventHandlers;
using Equinor.ProCoSys.IPO.Command.ICalendar;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.CertificateAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.SettingAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.Client;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Certificate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.ExportIPOs;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Equinor.ProCoSys.IPO.WebApi.Excel;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus.Receiver;
using Equinor.ProCoSys.PcsServiceBus.Receiver.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.IPO.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
            services.Configure<LibraryApiOptions>(configuration.GetSection("LibraryApi"));
            services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            services.Configure<IpoAuthenticatorOptions>(configuration.GetSection("Authenticator"));
            services.Configure<MeetingOptions>(configuration.GetSection("Meetings"));
            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services
                .Configure<GraphOptions>(configuration.GetSection("Graph"))
                .Configure<GraphOptions>(x => x.TenantId = configuration.GetValue<string>("TenantId"));
            services.Configure<SynchronizationOptions>(configuration.GetSection("Synchronization"));

            services.AddDbContext<IPOContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IPOContext");
                options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });


            // Hosted services
            services.AddHostedService<TimedSynchronization>();

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Transient - Created each time it is requested from the service container

            // Scoped - Created once per client request (connection)
            services.AddScoped<ITelemetryClient, ApplicationInsightsTelemetryClient>();
            services.AddScoped<IAccessValidator, AccessValidator>();
            services.AddScoped<IProjectAccessChecker, ProjectAccessChecker>();
            services.AddScoped<IProjectChecker, ProjectChecker>();
            services.AddScoped<IInvitationHelper, InvitationHelper>();
            services.AddScoped<IEventDispatcher, EventDispatcher>();
            services.AddScoped<IUnitOfWork>(x => x.GetRequiredService<IPOContext>());
            services.AddScoped<IReadOnlyContext, IPOContext>();
            services.AddScoped<IBusReceiverService, BusReceiverService>();
            services.AddScoped<ICertificateEventProcessorService, CertificateEventProcessorService>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<ILocalPersonRepository, LocalPersonRepository>();
            services.AddScoped<IInvitationRepository, InvitationRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ISettingRepository, SettingRepository>();
            services.AddScoped<IOutstandingIpoRepository, OutstandingIpoRepository>();
            services.AddScoped<IExportIpoRepository, ExportIpoRepository>();
            services.AddScoped<ICertificateRepository, CertificateRepository>();

            services.AddScoped<ISynchronizationService, SynchronizationService>();
            services.AddScoped<IAuthenticatorOptions, AuthenticatorOptions>();
            services.AddScoped<LibraryApiAuthenticator>();
            services.AddScoped<ILibraryApiAuthenticator>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            services.AddScoped<IBearerTokenSetter>(x => x.GetRequiredService<LibraryApiAuthenticator>());
            services.AddScoped<ILibraryApiClient, LibraryApiClient>();
            services.AddScoped<IProjectApiService, MainApiProjectService>();
            services.AddScoped<IAzureBlobService, AzureBlobService>();
            services.AddScoped<ICertificateApiService, MainApiCertificateService>();
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
            services.AddSingleton<IBusReceiverServiceFactory, ScopedBusReceiverServiceFactory>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<ICalendarService, CalendarService>();
        }
    }
}
