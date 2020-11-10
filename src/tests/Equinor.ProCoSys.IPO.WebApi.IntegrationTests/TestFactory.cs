using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Permission;
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
    public class TestFactory : WebApplicationFactory<Startup>
    {
        private readonly string _signerOid = "00000000-0000-0000-0000-000000000001";
        private readonly string _plannerOid = "00000000-0000-0000-0000-000000000002";
        private readonly string _preserverOid = "00000000-0000-0000-0000-000000000003";
        private readonly string _hackerOid = "00000000-0000-0000-0000-000000000666";
        private readonly string _integrationTestEnvironment = "IntegrationTests";
        private readonly string _connectionString;
        private readonly string _configPath;
        private readonly Dictionary<string, ITestUser> _testUsers = new Dictionary<string, ITestUser>();
        private readonly List<Action> _teardownList = new List<Action>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private readonly Mock<IPlantApiService> _plantApiServiceMock = new Mock<IPlantApiService>();
        private readonly Mock<IPermissionApiService> _permissionApiServiceMock = new Mock<IPermissionApiService>();
        public readonly Mock<IFusionMeetingClient> FusionMeetingClientMock = new Mock<IFusionMeetingClient>();

        public static string AnonymousUser => "NN";
        public static string SignerUser => "Sigurd Signer";
        public static string PlannerUser => "Pernilla Planner";
        public static string ViewerUser => "Vidar Viewer";
        public static string HackerUser => "Harry Hacker";
        public static string PlantWithAccess => SeedingData.Plant;
        public static string PlantWithoutAccess => "PCS$PLANT999";
        public static string UnknownPlant => "UNKNOWN_PLANT";
        public static string ProjectWithAccess => SeedingData.ProjectName;
        public static string ProjectWithoutAccess => "Project999";
        public static string AValidRowVersion => "AAAAAAAAAAA=";

        public SeedingData SeedingData { get; }

        public TestFactory()
        {
            SeedingData = new SeedingData();

            var projectDir = Directory.GetCurrentDirectory();
            _connectionString = GetTestDbConnectionString(projectDir);
            _configPath = Path.Combine(projectDir, "appsettings.json");

            SetupTestUsers();
        }

        public HttpClient GetClientForPlant(string user, string plant)
        {
            var testUser = _testUsers[user];
            
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

                // Add mocks to external resources here
                services.AddScoped(serviceProvider => _plantApiServiceMock.Object);
                services.AddScoped(serviceProvider => _permissionApiServiceMock.Object);
                services.AddScoped(serviceProvider => FusionMeetingClientMock.Object);
                // todo Mock ICommPkgApiService, IMcPkgApiService, IPersonApiService and IFunctionalRoleApiService ++
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

                    dbContext.Seed(scope.ServiceProvider, SeedingData);
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
            var dbName = "IntegrationTestsDB";
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
            => _testUsers.Add(HackerUser,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = HackerUser, 
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
            => _testUsers.Add(ViewerUser,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = ViewerUser,
                            Oid = _preserverOid
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
            => _testUsers.Add(SignerUser,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = SignerUser,
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
            => _testUsers.Add(PlannerUser,
                new TestUser
                {
                    Profile =
                        new TestProfile
                        {
                            FullName = PlannerUser,
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

        private void AddAnonymousUser() => _testUsers.Add(AnonymousUser, new TestUser());

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
