﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Email;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Me;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
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
using ProCoSysProject = Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission.ProCoSysProject;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public sealed class TestFactory : WebApplicationFactory<Startup>
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

        private readonly Mock<IPlantApiService> _plantApiServiceMock = new Mock<IPlantApiService>();
        private readonly Mock<IPermissionApiService> _permissionApiServiceMock = new Mock<IPermissionApiService>();
        public readonly Mock<ICurrentUserProvider> CurrentUserProviderMock = new Mock<ICurrentUserProvider>();
        public readonly Mock<IFusionMeetingClient> FusionMeetingClientMock = new Mock<IFusionMeetingClient>();
        public readonly Mock<IOptionsMonitor<MeetingOptions>> MeetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();
        public readonly Mock<ICommPkgApiService> CommPkgApiServiceMock = new Mock<ICommPkgApiService>();
        public readonly Mock<IMcPkgApiService> McPkgApiServiceMock = new Mock<IMcPkgApiService>();
        public readonly Mock<IPersonApiService> PersonApiServiceMock = new Mock<IPersonApiService>();
        public readonly Mock<IFunctionalRoleApiService> FunctionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
        public readonly Mock<IProjectApiService> ProjectApiServiceMock = new Mock<IProjectApiService>();
        public readonly Mock<IBlobStorage> BlobStorageMock = new Mock<IBlobStorage>();
        public readonly Mock<IPcsBusSender> PcsBusSenderMock = new Mock<IPcsBusSender>();
        public readonly Mock<IMeApiService> MeApiServiceMock = new Mock<IMeApiService>();
        public readonly Mock<IEmailService> EmailServiceMock = new Mock<IEmailService>();

        public static string PlantWithAccess => KnownTestData.Plant;
        public static string PlantWithoutAccess => "PCS$PLANT999";
        public static string UnknownPlant => "UNKNOWN_PLANT";
        public static string ProjectWithAccess => KnownTestData.ProjectName;
        public static string ProjectWithoutAccess => "Project999";
        public static string AValidRowVersion => "AAAAAAAAAAA=";
        public static string WrongButValidRowVersion => "AAAAAAAAAAA=";

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
            
            if (testUser.Profile != null)
            {
                Instance
                    .CurrentUserProviderMock
                    .Setup(x => x.GetCurrentUserOid())
                    .Returns(Guid.Parse(testUser.Profile.Oid));
            }

            // Need to change what the mock returns each time since the factory share the same registered mocks
            SetupPlantMock(testUser.ProCoSysPlants);
            
            SetupPermissionMock(plant, 
                testUser.ProCoSysPermissions,
                testUser.ProCoSysProjects);
            
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
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication()
                    .AddScheme<IntegrationTestAuthOptions, IntegrationTestAuthHandler>(
                        IntegrationTestAuthHandler.TestAuthenticationScheme, opts => { });

                services.PostConfigureAll<JwtBearerOptions>(jwtBearerOptions =>
                    jwtBearerOptions.ForwardAuthenticate = IntegrationTestAuthHandler.TestAuthenticationScheme);

                // Add mocks to all external resources here
                services.AddScoped(serviceProvider => _plantApiServiceMock.Object);
                services.AddScoped(serviceProvider => _permissionApiServiceMock.Object);
                services.AddScoped(serviceProvider => FusionMeetingClientMock.Object);
                services.AddScoped(serviceProvider => MeetingOptionsMock.Object);
                services.AddScoped(serviceProvider => CommPkgApiServiceMock.Object);
                services.AddScoped(serviceProvider => McPkgApiServiceMock.Object);
                services.AddScoped(serviceProvider => PersonApiServiceMock.Object);
                services.AddScoped(serviceProvider => FunctionalRoleApiServiceMock.Object);
                services.AddScoped(serviceProvider => ProjectApiServiceMock.Object);
                services.AddScoped(serviceProvider => BlobStorageMock.Object);
                services.AddScoped(serviceProvider => PcsBusSenderMock.Object);
                services.AddScoped(serviceProvider => MeApiServiceMock.Object);
                services.AddScoped(serviceProvider => EmailServiceMock.Object);
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

            services.AddDbContext<IPOContext>(options => options.UseSqlServer(_connectionString));
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
                using (var dbContext = DatabaseContext(services))
                {
                    dbContext.Database.EnsureDeleted();
                }
            });

        private IPOContext DatabaseContext(IServiceCollection services)
        {
            services.AddDbContext<IPOContext>(options => options.UseSqlServer(_connectionString));

            var sp = services.BuildServiceProvider();
            _disposables.Add(sp);

            var spScope = sp.CreateScope();
            _disposables.Add(spScope);

            return spScope.ServiceProvider.GetRequiredService<IPOContext>();
        }

        private string GetTestDbConnectionString(string projectDir)
        {
            var dbName = "IntegrationTestsIPODb";
            var dbPath = Path.Combine(projectDir, $"{dbName}.mdf");
            
            // Set Initial Catalog to be able to delete database!
            return $"Server=(LocalDB)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=true;AttachDbFileName={dbPath}";
        }

        private void SetupPlantMock(List<ProCoSysPlant> plants)
            => _plantApiServiceMock.Setup(p => p.GetAllPlantsAsync()).Returns(Task.FromResult(plants));
        
        private void SetupPermissionMock(
            string plant,
            IList<string> proCoSysPermissions,
            IList<ProCoSysProject> proCoSysProjects)
        {
            _permissionApiServiceMock.Setup(p => p.GetPermissionsAsync(plant))
                .Returns(Task.FromResult(proCoSysPermissions));
                        
            _permissionApiServiceMock.Setup(p => p.GetAllOpenProjectsAsync(plant))
                .Returns(Task.FromResult(proCoSysProjects));
        }

        private void SetupTestUsers()
        {
            var commonProCoSysPlants = new List<ProCoSysPlant>
            {
                new ProCoSysPlant {Id = PlantWithAccess, HasAccess = true},
                new ProCoSysPlant {Id = PlantWithoutAccess}
            };

            var commonProCoSysProjects = new List<ProCoSysProject>
            {
                new ProCoSysProject {Name = ProjectWithAccess, HasAccess = true},
                new ProCoSysProject {Name = ProjectWithoutAccess}
            };

            AddAnonymousUser();

            AddSignerUser(commonProCoSysPlants, commonProCoSysProjects);
            
            AddPlannerUser(commonProCoSysPlants, commonProCoSysProjects);

            AddViewerUser(commonProCoSysPlants, commonProCoSysProjects);
    
            AddHackerUser(commonProCoSysProjects);

            AddContractorUser(commonProCoSysPlants, commonProCoSysProjects);

            AddAdminUser(commonProCoSysPlants, commonProCoSysProjects);

            AddCreatorUser(commonProCoSysPlants, commonProCoSysProjects); 

            var webHostBuilder = WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(IntegrationTestEnvironment);
                builder.ConfigureAppConfiguration((context, conf) => conf.AddJsonFile(_configPath));
            });

            foreach (var testUser in _testUsers.Values)
            {
                testUser.HttpClient = webHostBuilder.CreateClient();

                if (testUser.Profile != null)
                {
                    AuthenticateUser(testUser);
                }
            }
        }

        // Authenticated client without any permissions
        private void AddHackerUser(List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Hacker,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Harry",
                            LastName = "Hacker", 
                            Oid = HackerOid,
                            Email = "harry.hacker@pcs.pcs"
                        },
                    ProCoSysPlants = new List<ProCoSysPlant>
                    {
                        new ProCoSysPlant {Id = PlantWithAccess},
                        new ProCoSysPlant {Id = PlantWithoutAccess}
                    },
                    ProCoSysPermissions = new List<string>(),
                    ProCoSysProjects = commonProCoSysProjects
                });

        // Authenticated client with necessary permissions to VIEW invitations
        private void AddViewerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Viewer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Vidar",
                            LastName = " Viewer",
                            Oid = ViewerOid,
                            Email = "vidar.viewer@pcs.pcs"
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ
                    },
                    ProCoSysProjects = commonProCoSysProjects
                });

        // Authenticated user with necessary permissions to SIGN invitations (including completing and accepting)
        private void AddSignerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Signer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Sigurd",
                            LastName = "Signer",
                            UserName = "SigurdUserName",
                            Oid = SignerOid,
                            Email = "sigurd.signer@pcs.pcs"
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
                    {
                        Permissions.COMMPKG_READ,
                        Permissions.MCPKG_READ,
                        Permissions.PROJECT_READ,
                        Permissions.LIBRARY_FUNCTIONAL_ROLE_READ,
                        Permissions.USER_READ,
                        Permissions.IPO_READ,
                        Permissions.IPO_SIGN
                    },
                    ProCoSysProjects = commonProCoSysProjects
                });

        // Authenticated user with necessary permissions to CREATE, UPDATE AND CANCEL invitations
        private void AddPlannerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Planner,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Pernilla",
                            LastName = "Planner",
                            UserName = "PernillaUserName",
                            Oid = PlannerOid,
                            Email = "pernilla.planner@pcs.pcs"
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
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
                    ProCoSysProjects = commonProCoSysProjects
                });

        private void AddContractorUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Contractor,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Conte",
                            LastName = "Contractor",
                            UserName = "ContractorUserName",
                            Oid = ContractorOid,
                            Email = "conte.contractor@pcs.pcs"  
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
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
                    ProCoSysProjects = commonProCoSysProjects
                });
                
        // Authenticated user with all IPO permissions
        private void AddAdminUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Admin,
            new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                          FirstName = "Andrea",
                            LastName = "Admin",
                            UserName = "AndreaAdminUserName",
                            Oid = AdminOid,
                            Email = "andrea.admin@pcs.pcs"
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
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
                    ProCoSysProjects = commonProCoSysProjects
                });

        private void AddCreatorUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Creator,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FirstName = "Bill",
                            LastName = "Shankly",
                            UserName = "ShanklyCreator",
                            Oid = CreatorOid,
                            Email = "bill.shankly@pcs.pcs"
                        },
                    ProCoSysPlants = commonProCoSysPlants,
                    ProCoSysPermissions = new List<string>
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
                    ProCoSysProjects = commonProCoSysProjects
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
