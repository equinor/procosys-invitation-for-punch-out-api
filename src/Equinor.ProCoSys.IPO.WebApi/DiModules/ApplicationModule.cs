using System;
using System.Text.Json.Serialization;
using Azure.Core;
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
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
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
using Equinor.ProCoSys.IPO.Fam;
using Equinor.ProCoSys.IPO.ForeignApi;
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
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs;
using Equinor.ProCoSys.IPO.Query;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Equinor.ProCoSys.IPO.WebApi.Authorizations.TokenCredentials;
using Equinor.ProCoSys.IPO.WebApi.Excel;
using Equinor.ProCoSys.IPO.WebApi.Extensions;
using Equinor.ProCoSys.IPO.WebApi.MassTransit;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Synchronization;
using Equinor.ProCoSys.PcsServiceBus.Receiver;
using Equinor.ProCoSys.PcsServiceBus.Receiver.Interfaces;
using Fam.Core.EventHubs.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Equinor.ProCoSys.IPO.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(
            this IServiceCollection services,
            IConfiguration configuration,
            TokenCredential credential)
        {
            services.Configure<MainApiOptions>(configuration.GetSection("MainApi"));
            services.Configure<MainApiAuthenticatorOptions>(configuration.GetSection("AzureAd"));
            services.Configure<LibraryApiOptions>(configuration.GetSection("LibraryApi"));
            services.Configure<FamOptions>(configuration.GetSection("Fam"));

            services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            services.Configure<ApplicationOptions>(configuration.GetSection("Application"));
            services.Configure<MeetingOptions>(configuration.GetSection("Meetings"));
            services.Configure<EmailOptions>(configuration.GetSection("Email"));
            services.Configure<SynchronizationOptions>(configuration.GetSection("Synchronization"));

            services.AddDbContext<IPOContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("IPOContext");
                options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            });

            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<IPOContext>(o =>
                {
                    o.UseSqlServer();
                    o.UseBusOutbox();
                });

                x.UsingAzureServiceBus((_, cfg) =>
                {
                    var serviceBusNamespace = configuration.GetValue<string>("ServiceBus:Namespace");
                    var serviceUri = new Uri($"sb://{serviceBusNamespace}.servicebus.windows.net/");

                    cfg.Host(serviceUri, host =>
                    {
                        host.TokenCredential = credential;
                    });

                    cfg.MessageTopology.SetEntityNameFormatter(new IpoEntityNameFormatter());
                    cfg.UseRawJsonSerializer();
                    cfg.ConfigureJsonSerializerOptions(opts =>
                    {
                        opts.Converters.Add(new JsonStringEnumConverter());

                        // Set it to null to use the default .NET naming convention (PascalCase)
                        opts.PropertyNamingPolicy = null;
                        return opts;
                    });

                    cfg.AutoStart = true;
                });
            });
            services.AddEventHubProducer(configBuilder
                => configuration.Bind("EventHubProducerConfig", configBuilder));

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
            services.AddScoped<ICreateEventHelper, CreateEventHelper>();

            services.AddScoped<ISynchronizationService, SynchronizationService>();
            services.AddScoped<ILibraryApiForUserClient, LibraryApiClientForUser>();
            services.AddScoped<IProjectApiForApplicationService, MainApiForApplicationProjectService>();
            services.AddScoped<IProjectApiForUsersService, MainApiForUsersProjectService>();
            services.AddScoped<IAzureBlobService, AzureBlobService>();
            services.AddScoped<ICertificateApiService, MainApiCertificateService>();
            services.AddScoped<ICommPkgApiForApplicationService, MainApiForApplicationCommPkgService>();
            services.AddScoped<ICommPkgApiForUserService, MainApiForUserCommPkgService>();
            services.AddScoped<IMcPkgApiForApplicationService, MainApiForApplicationMcPkgService>();
            services.AddScoped<IMcPkgApiForUserService, MainApiForUserMcPkgService>();
            services.AddScoped<IFunctionalRoleApiService, LibraryApiFunctionalRoleService>();
            services.AddScoped<IPersonApiService, MainApiPersonService>();
            services.AddScoped<IMeApiService, MainApiMeService>();

            services.AddScoped<IInvitationValidator, InvitationValidator>();
            services.AddScoped<IRowVersionValidator, RowVersionValidator>();
            services.AddScoped<ISavedFilterValidator, SavedFilterValidator>();

            services.AddScoped<IExcelConverter, ExcelConverter>();
            services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
            services.AddScoped<IFamRepository, FamRepository>();

            // Singleton - Created the first time they are requested
            services.AddSingleton<IBusReceiverServiceFactory, ScopedBusReceiverServiceFactory>();
            services.AddSingleton<ICalendarService, CalendarService>();
            services.AddSingleton<IQueryUserDelegationProvider, UserDelegationProvider>();

            services.AddTransient<IEmailService, IpoEmailService>();

            AddHttpClients(services);
            
            services.AddTransient<ITokenCredential>(_ => new IpoTokenCredential(credential));
            AddMailCredential(services, configuration);
            AddFamCredential(services, configuration);
        }

        private static void AddHttpClients(IServiceCollection services)
        {
            services.AddTransient<LibraryApiForUserTokenHandler>();

            services.AddHttpClient(LibraryApiClientForUser.ClientName)
                .AddHttpMessageHandler<LibraryApiForUserTokenHandler>();
        }

        private static void AddMailCredential(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.IsDevOnLocalhost())
            {
                // The default credentials use federated credentials for authentication.
                // That will not work on a local dev machine.
                // Replacing the default authentication with a certificate authentication.
                services.AddTransient<IMailCredential, MailCertificateCredential>();
                return;
            }

            services.AddTransient<IMailCredential, MailDefaultCredential>();
        }

        private static void AddFamCredential(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.IsDevOnLocalhost())
            {
                // The default credentials use federated credentials for authentication.
                // That will not work on a local dev machine.
                // Replacing the default authentication with a certificate authentication.
                services.AddTransient<IFamCredential, FamCertificateCredential>();
                return;
            }

            services.AddTransient<IFamCredential, FamDefaultCredential>();
        }
    }
}
