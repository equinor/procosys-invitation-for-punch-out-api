using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.BlobStorage;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using Fusion.Integration.Meeting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public sealed class TestFactory : WebApplicationFactory<Startup>
    {
        private readonly string _signerOid = "00000000-0000-0000-0000-000000000001";
        private readonly string _plannerOid = "00000000-0000-0000-0000-000000000002";
        private readonly string _viewerOid = "00000000-0000-0000-0000-000000000003";
        private readonly string _hackerOid = "00000000-0000-0000-0000-000000000666";

        private readonly string _integrationTestEnvironment = "IntegrationTests";
        private readonly string _connectionString;
        private readonly string _configPath;
        private readonly Dictionary<UserType, ITestUser> _testUsers = new Dictionary<UserType, ITestUser>();
        private readonly List<Action> _teardownList = new List<Action>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private readonly Mock<IPlantApiService> _plantApiServiceMock = new Mock<IPlantApiService>();
        private readonly Mock<IPermissionApiService> _permissionApiServiceMock = new Mock<IPermissionApiService>();
        public readonly Mock<IFusionMeetingClient> FusionMeetingClientMock = new Mock<IFusionMeetingClient>();
        public readonly Mock<ICommPkgApiService> CommPkgApiServiceMock = new Mock<ICommPkgApiService>();
        public readonly Mock<IMcPkgApiService> McPkgApiServiceMock = new Mock<IMcPkgApiService>();
        public readonly Mock<IPersonApiService> PersonApiServiceMock = new Mock<IPersonApiService>();
        public readonly Mock<IFunctionalRoleApiService> FunctionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
        public readonly Mock<IBlobStorage> BlobStorageMock = new Mock<IBlobStorage>();

        public static string PlantWithAccess => KnownTestData.Plant;
        public static string PlantWithoutAccess => "PCS$PLANT999";
        public static string UnknownPlant => "UNKNOWN_PLANT";
        public static string ProjectWithAccess => KnownTestData.ProjectName;
        public static string ProjectWithoutAccess => "Project999";
        public static string AValidRowVersion => "AAAAAAAAAAA=";

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
            
            // Need to change what the mock returns each time since the factory share the same registered mocks
            SetupPlantMock(testUser.ProCoSysPlants);
            
            SetupPermissionMock(plant, 
                testUser.ProCoSysPermissions,
                testUser.ProCoSysProjects);
            
            UpdatePlantInHeader(testUser.HttpClient, plant);
            
            return testUser.HttpClient;
        }

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
                services.AddScoped(serviceProvider => CommPkgApiServiceMock.Object);
                services.AddScoped(serviceProvider => McPkgApiServiceMock.Object);
                services.AddScoped(serviceProvider => PersonApiServiceMock.Object);
                services.AddScoped(serviceProvider => FunctionalRoleApiServiceMock.Object);
                services.AddScoped(serviceProvider => BlobStorageMock.Object);
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
            
            var webHostBuilder = WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(_integrationTestEnvironment);
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
                            FullName = "Harry Hacker", 
                            Oid = _hackerOid
                        },
                    ProCoSysPlants = new List<ProCoSysPlant>
                    {
                        new ProCoSysPlant {Id = PlantWithAccess},
                        new ProCoSysPlant {Id = PlantWithoutAccess}
                    },
                    ProCoSysPermissions = new List<string>(),
                    ProCoSysProjects = commonProCoSysProjects
                });

        // Authenticated client with necessary permissions to view IPO
        private void AddViewerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Viewer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = "Vidar Viewer",
                            Oid = _viewerOid
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

        // Authenticated user with necessary permissions to Sign Invitations
        private void AddSignerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Signer,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = "Sigurd Signer",
                            Oid = _signerOid
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
                        Permissions.IPO_SIGN,
                        Permissions.IPO_ATTACHFILE,
                        Permissions.IPO_DETACHFILE
                    },
                    ProCoSysProjects = commonProCoSysProjects
                });

        // Authenticated user with necessary permissions to Create and Update Invitations
        private void AddPlannerUser(
            List<ProCoSysPlant> commonProCoSysPlants,
            List<ProCoSysProject> commonProCoSysProjects)
            => _testUsers.Add(UserType.Planner,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = "Pernilla Planner",
                            Oid = _plannerOid
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
            => user.HttpClient.DefaultRequestHeaders.Add("Authorization", CreateBearerToken(user.Profile));

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
        
        /// <summary>
        /// Wraps profile by serializing, encoding and then converting to base 64 string.
        /// "Bearer" is also added, making it ready to be added as Authorization header
        /// </summary>
        /// <param name="profile">The instance of the token to be wrapped</param>
        /// <returns>Serialized, encoded string ready for authorization header</returns>
        private string CreateBearerToken(TestProfile profile)
        {
            var serialized = JsonConvert.SerializeObject(profile);
            var tokenBytes = Encoding.UTF8.GetBytes(serialized);
            var tokenString = Convert.ToBase64String(tokenBytes);

            return $"Bearer {tokenString}";
        }
    }
}
