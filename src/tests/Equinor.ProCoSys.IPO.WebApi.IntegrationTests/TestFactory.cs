using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.BlobStorage;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Command.EventHandlers.IntegrationEvents;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Fam;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using Equinor.ProCoSys.PcsServiceBus.Sender.Interfaces;
using Fusion.Integration.Meeting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using AuthProCoSysPerson = Equinor.ProCoSys.Auth.Person.ProCoSysPerson;
using IAuthPersonApiService = Equinor.ProCoSys.Auth.Person.IPersonApiService;
using IMainPersonApiService = Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person.IPersonApiService;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private const string SignerOid = "00000000-0000-0000-0000-000000000001";
        private const string PlannerOid = "00000000-0000-0000-0000-000000000002";
        private const string ViewerOid = "00000000-0000-0000-0000-000000000003";
        private const string CreatorOid = "00000000-0000-0000-0000-000000000004";
        private const string HackerOid = "00000000-0000-0000-0000-000000000666";
        private const string ContractorOid = "00000000-0000-0000-0000-000000000007";
        private const string AdminOid = "00000000-0000-0000-0000-000000000008";

        private const string IntegrationTestEnvironment = "IntegrationTests";
        private readonly string _connectionString;
        private readonly string _configPath;
        private readonly Dictionary<UserType, ITestUser> _testUsers = new Dictionary<UserType, ITestUser>();
        private readonly List<Action> _teardownList = new List<Action>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private readonly Mock<IAuthPersonApiService> _authPersonApiServiceMock = new Mock<IAuthPersonApiService>();
        private readonly Mock<IPermissionApiService> _permissionApiServiceMock = new Mock<IPermissionApiService>();
        public readonly Mock<ICurrentUserProvider> CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
        public readonly Mock<IFusionMeetingClient> FusionMeetingClientMock = new Mock<IFusionMeetingClient>();
        public readonly Mock<IOptionsMonitor<MeetingOptions>> MeetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();
        public readonly Mock<IOptionsMonitor<FamOptions>> FamOptionsMock = new Mock<IOptionsMonitor<FamOptions>>();
        public readonly Mock<ICommPkgApiService> CommPkgApiServiceMock = new Mock<ICommPkgApiService>();
        public readonly Mock<IMcPkgApiForUserService> McPkgApiServiceMock = new Mock<IMcPkgApiForUserService>();
        public readonly Mock<IMainPersonApiService> MainPersonApiServiceMock = new Mock<IMainPersonApiService>();
        public readonly Mock<IFunctionalRoleApiService> FunctionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
        public readonly Mock<IProjectApiForUsersService> ProjectApiServiceMock = new Mock<IProjectApiForUsersService>();
        public readonly Mock<IAzureBlobService> BlobStorageMock = new Mock<IAzureBlobService>();
        public readonly Mock<IPcsBusSender> PcsBusSenderMock = new Mock<IPcsBusSender>();
        public readonly Mock<IMeApiService> MeApiServiceMock = new Mock<IMeApiService>();
        public readonly Mock<IEmailService> EmailServiceMock = new Mock<IEmailService>();
        public readonly Mock<IIntegrationEventPublisher> IntegrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();
        public readonly Mock<ICreateEventHelper> CreateEventHelperMock = new Mock<ICreateEventHelper>();


        public static string PlantWithAccess => KnownTestData.Plant;
        public static string PlantWithoutAccess => "PCS$PLANT999";
        public static string UnknownPlant => "UNKNOWN_PLANT";
        public static string ProjectWithAccess => KnownTestData.ProjectName;
        public static string ProjectWithoutAccess => "Project999";
        public static string AValidRowVersion => "AAAAAAAAAAA=";
        public static string WrongButValidRowVersion => "AAAAAAAAAAA=";
        public const string SendToFamCorrectApiKey = "Correct api key";

        public KnownTestData KnownTestData { get; }

        #region singleton implementation
        private static TestFactory s_instance;
        private static readonly object s_padlock = new object();

        public static TestFactory Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_padlock)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new TestFactory();
                        }
                    }
                }

                return s_instance;
            }
        }

        private TestFactory()
        {
            KnownTestData = new KnownTestData();

            var projectDir = Directory.GetCurrentDirectory();
            _connectionString = GetTestDbConnectionString(projectDir);
            _configPath = Path.Combine(projectDir, "appsettings.json");

            SetupTestUsers();
        }
        #endregion

        public HttpClient GetHttpClient(UserType userType, string plant)
        {
            var testUser = _testUsers[userType];

            SetupPermissionMock(plant, testUser);

            UpdatePlantInHeader(testUser.HttpClient, plant);

            return testUser.HttpClient;
        }

        public ITestUser GetTestUserForUserType(UserType userType) => _testUsers[userType];

        public new void Dispose()
        {
            // Run teardown
            foreach (var action in _teardownList)
            {
                action();
            }

            foreach (var testUser in _testUsers)
            {
                testUser.Value.HttpClient.Dispose();
            }

            foreach (var disposable in _disposables)
            {
                try { disposable.Dispose(); } catch { /* Ignore */ }
            }

            lock (s_padlock)
            {
                s_instance = null;
            }

            base.Dispose();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(IntegrationTestEnvironment);
            builder.ConfigureAppConfiguration((context, conf) => conf.AddJsonFile(_configPath));
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication()
                      .AddScheme<IntegrationTestAuthOptions, IntegrationTestAuthHandler>(
                          IntegrationTestAuthHandler.TestAuthenticationScheme, opts => { });

                services.PostConfigureAll<JwtBearerOptions>(jwtBearerOptions =>
                      jwtBearerOptions.ForwardAuthenticate = IntegrationTestAuthHandler.TestAuthenticationScheme);

                // Add mocks to all external resources here
                services.AddScoped(_ => _authPersonApiServiceMock.Object);
                services.AddScoped(_ => _permissionApiServiceMock.Object);
                services.AddScoped(_ => FusionMeetingClientMock.Object);
                services.AddScoped(_ => MeetingOptionsMock.Object);
                services.AddScoped(_ => FamOptionsMock.Object);
                services.AddScoped(_ => CommPkgApiServiceMock.Object);
                services.AddScoped(_ => McPkgApiServiceMock.Object);
                services.AddScoped(_ => MainPersonApiServiceMock.Object);
                services.AddScoped(_ => FunctionalRoleApiServiceMock.Object);
                services.AddScoped(_ => ProjectApiServiceMock.Object);
                services.AddScoped(_ => BlobStorageMock.Object);
                services.AddScoped(_ => PcsBusSenderMock.Object);
                services.AddScoped(_ => MeApiServiceMock.Object);
                services.AddScoped(_ => EmailServiceMock.Object);
                services.AddScoped(_ => IntegrationEventPublisherMock.Object);
                services.AddScoped(_ => CreateEventHelperMock.Object);
            });

            builder.ConfigureServices(services =>
            {
                ReplaceRealDbContextWithTestDbContext(services);

                CreateSeededTestDatabase(services);

                EnsureTestDatabaseDeletedAtTeardown(services);
            });
        }

        private void ReplaceRealDbContextWithTestDbContext(IServiceCollection services)
        {
            var descriptor = services.SingleOrDefault
                (d => d.ServiceType == typeof(DbContextOptions<IPOContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<IPOContext>(options
                => options.UseSqlServer(_connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        }

        private void CreateSeededTestDatabase(IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<IPOContext>();

                    dbContext.Database.EnsureDeleted();

                    dbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

                    dbContext.CreateNewDatabaseWithCorrectSchema();
                    var migrations = dbContext.Database.GetPendingMigrations();
                    if (migrations.Any())
                    {
                        dbContext.Database.Migrate();
                    }

                    dbContext.Seed(scope.ServiceProvider, KnownTestData);
                }
            }
        }

        private void EnsureTestDatabaseDeletedAtTeardown(IServiceCollection services)
            => _teardownList.Add(() =>
            {
                using var sp = services.BuildServiceProvider();
                using var dbContext = sp.GetRequiredService<IPOContext>();
                    
                dbContext.Database.EnsureDeleted();
            });

        private string GetTestDbConnectionString(string projectDir)
        {
            var dbName = "IntegrationTestsIPODb";
            var dbPath = Path.Combine(projectDir, $"{dbName}.mdf");

            // Set Initial Catalog to be able to delete database!
            return $"Server=(LocalDB)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=true;AttachDbFileName={dbPath}";
        }

        private void SetupPermissionMock(string plant, ITestUser testUser)
        {
            if (testUser.Profile != null)
            {
                Instance
                    .CurrentUserProviderMock.Setup(x => x.GetCurrentUserOid())
                    .Returns(Guid.Parse(testUser.Profile.Oid));
            }

            _permissionApiServiceMock.Setup(p => p.GetPermissionsForCurrentUserAsync(plant, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testUser.Permissions));

            _permissionApiServiceMock.Setup(p => p.GetAllOpenProjectsForCurrentUserAsync(plant, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testUser.AccessableProjects));
            
            _permissionApiServiceMock.Setup(p => p.GetRestrictionRolesForCurrentUserAsync(plant, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(testUser.Restrictions));
        }

        private void SetupTestUsers()
        {
            var accessablePlants = new List<AccessablePlant>
            {
                new AccessablePlant {Id = PlantWithAccess, HasAccess = true},
                new AccessablePlant {Id = PlantWithoutAccess}
            };

            var accessableProjects = new List<AccessableProject>
            {
                new AccessableProject {Name = ProjectWithAccess, HasAccess = true},
                new AccessableProject {Name = ProjectWithoutAccess}
            };
            
            var restrictions = new List<string>
            {
                ClaimsTransformation.NoRestrictions
            };

            AddAnonymousUser();

            AddSignerUser(accessablePlants, accessableProjects, restrictions);

            AddPlannerUser(accessablePlants, accessableProjects, restrictions);

            AddViewerUser(accessablePlants, accessableProjects, restrictions);

            AddHackerUser();

            AddContractorUser(accessablePlants, accessableProjects, restrictions);

            AddAdminUser(accessablePlants, accessableProjects, restrictions);

            AddCreatorUser(accessablePlants, accessableProjects, restrictions);

            SetupProCoSysServiceMocks();

            CreateAuthenticatedHttpClients();
        }

        private void CreateAuthenticatedHttpClients()
        {
            foreach (var testUser in _testUsers.Values)
            {
                testUser.HttpClient = CreateClient();

                if (testUser.Profile != null)
                {
                    AuthenticateUser(testUser);
                }
            }
        }

        private void SetupProCoSysServiceMocks()
        {
            foreach (var testUser in _testUsers.Values.Where(t => t.Profile != null))
            {
                if (testUser.AuthProCoSysPerson != null)
                {
                    _authPersonApiServiceMock.Setup(p => p.TryGetPersonByOidAsync(new Guid(testUser.Profile.Oid), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(testUser.AuthProCoSysPerson));
                }
                else
                {
                    _authPersonApiServiceMock.Setup(p => p.TryGetPersonByOidAsync(new Guid(testUser.Profile.Oid), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.FromResult((AuthProCoSysPerson)null));
                }
                _permissionApiServiceMock.Setup(p => p.GetAllPlantsForUserAsync(new Guid(testUser.Profile.Oid), It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(testUser.AccessablePlants));
            }

            // Need to mock getting info for current application from Main. This to satisfy VerifyIpoApiClientExists middelware
            var config = new ConfigurationBuilder().AddJsonFile(_configPath).Build();
            var ipoApiObjectId = config["Application:ObjectId"];
            _authPersonApiServiceMock.Setup(p => p.TryGetPersonByOidAsync(new Guid(ipoApiObjectId), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new AuthProCoSysPerson
                {
                    AzureOid = ipoApiObjectId,
                    FirstName = "Ipo",
                    LastName = "API",
                    Email = "ipo@pcs.net",
                    UserName = "IA"
                }));
        }

        // Authenticated client without any permissions
        private void AddHackerUser()
            => _testUsers.Add(UserType.Hacker,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Harry",
                            LastName = "Hacker",
                            UserName = "HH",
                            Oid = HackerOid,
                            Email = "harry.hacker@pcs.pcs"
                        },
                    AccessablePlants = new List<AccessablePlant>
                    {
                        new AccessablePlant {Id = PlantWithAccess, HasAccess = false},
                        new AccessablePlant {Id = PlantWithoutAccess, HasAccess = false}
                    },
                    Permissions = new List<string>(),
                    AccessableProjects = new List<AccessableProject>(),
                    Restrictions = new List<string>()
                });

        // Authenticated client with necessary permissions to VIEW invitations
        private void AddViewerUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Viewer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Vidar",
                            LastName = " Viewer",
                            UserName = "VV",
                            Oid = ViewerOid,
                            Email = "vidar.viewer@pcs.pcs"
                        },
                    AccessablePlants = accessablePlants,
                    Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ
                    },
                    AccessableProjects = accessableProjects,
                    Restrictions = commonProCoSysRestrictions
                });

        // Authenticated user with necessary permissions to SIGN invitations (including completing and accepting)
        private void AddSignerUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Signer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Sigurd",
                            LastName = "Signer",
                            UserName = "SS",
                            Oid = SignerOid,
                            Email = "sigurd.signer@pcs.pcs"
                        },
                    AccessablePlants = accessablePlants,
                    Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_SIGN
                    },
                    AccessableProjects = accessableProjects,
                    Restrictions = commonProCoSysRestrictions
                });

        // Authenticated user with necessary permissions to CREATE, UPDATE AND CANCEL invitations
        private void AddPlannerUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Planner,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Pernilla",
                            LastName = "Planner",
                            UserName = "PP",
                            Oid = PlannerOid,
                            Email = "pernilla.planner@pcs.pcs"
                        },
                    AccessablePlants = accessablePlants,
                    Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_WRITE,
                        Permissions.IPO_CREATE,
                        Permissions.IPO_DELETE,
                        Permissions.IPO_ATTACHFILE,
                        Permissions.IPO_DETACHFILE,
                        Permissions.IPO_VOIDUNVOID,
                    },
                    AccessableProjects = accessableProjects,
                    Restrictions = commonProCoSysRestrictions
                });

        private void AddContractorUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Contractor,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Conte",
                            LastName = "Contractor",
                            UserName = "CC",
                            Oid = ContractorOid,
                            Email = "conte.contractor@pcs.pcs"
                        },
                    AccessablePlants = accessablePlants,
                    Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_WRITE,
                        Permissions.IPO_VOIDUNVOID,
                    },
                    AccessableProjects = accessableProjects,
                    Restrictions = commonProCoSysRestrictions
                });

        // Authenticated user with all IPO permissions
        private void AddAdminUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Admin,
            new TestUser
            {
                Profile =
                        new TestProfile
                        {
                            FirstName = "Andrea",
                            LastName = "Admin",
                            UserName = "AA",
                            Oid = AdminOid,
                            Email = "andrea.admin@pcs.pcs"
                        },
                AccessablePlants = accessablePlants,
                Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_WRITE,
                        Permissions.IPO_CREATE,
                        Permissions.IPO_DELETE,
                        Permissions.IPO_ATTACHFILE,
                        Permissions.IPO_DETACHFILE,
                        Permissions.IPO_VOIDUNVOID,
                        Permissions.IPO_ADMIN,
                    },
                AccessableProjects = accessableProjects,
                Restrictions = commonProCoSysRestrictions
            });

        private void AddCreatorUser(List<AccessablePlant> accessablePlants,
            List<AccessableProject> accessableProjects, List<string> commonProCoSysRestrictions)
            => _testUsers.Add(UserType.Creator,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Bill",
                            LastName = "Shankly",
                            UserName = "BS",
                            Oid = CreatorOid,
                            Email = "bill.shankly@pcs.pcs"
                        },
                    AccessablePlants = accessablePlants,
                    Permissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_WRITE,
                        Permissions.IPO_CREATE,
                        Permissions.IPO_DELETE,
                        Permissions.IPO_ATTACHFILE,
                        Permissions.IPO_DETACHFILE,
                        Permissions.IPO_VOIDUNVOID
                    },
                    AccessableProjects = accessableProjects,
                    Restrictions = commonProCoSysRestrictions
                });

        private void AddAnonymousUser() => _testUsers.Add(UserType.Anonymous, new TestUser());

        private void AuthenticateUser(ITestUser user)
            => user.HttpClient.DefaultRequestHeaders.Add("Authorization", user.Profile.CreateBearerToken());

        private void UpdatePlantInHeader(HttpClient client, string plant)
        {
            if (client.DefaultRequestHeaders.Contains(CurrentPlantMiddleware.PlantHeader))
            {
                client.DefaultRequestHeaders.Remove(CurrentPlantMiddleware.PlantHeader);
            }

            if (!string.IsNullOrEmpty(plant))
            {
                client.DefaultRequestHeaders.Add(CurrentPlantMiddleware.PlantHeader, plant);
            }
        }
    }
}
