using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Permission;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public abstract class ReadOnlyTestsBaseInMemory : ReadOnlyTestsBase
    {
        protected override DbContextOptions<IPOContext> CreateDbContextOptions()
        {
            return new DbContextOptionsBuilder<IPOContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        protected override IPOContext CreateDbContext(DbContextOptions<IPOContext> dbContextOptions) => new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
    }

    //Used when unit tests are covering logic where queryies are executed through Dapper. Dapper can not connect to EF Core In Memory Db. 
    public abstract class ReadOnlyTestsBaseSqlLiteInMemory : ReadOnlyTestsBase
    {
        protected override DbContextOptions<IPOContext> CreateDbContextOptions()
        {
            var sqlLiteConnection = new SqliteConnection("Filename=:memory:");
            sqlLiteConnection.Open();

            return new DbContextOptionsBuilder<IPOContext>()
                .UseSqlite(sqlLiteConnection)
                .Options;
        }

        protected override IPOContext CreateDbContext(DbContextOptions<IPOContext> dbContextOptions) => new IPOContextSqlLite(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
    }

    public abstract class ReadOnlyTestsBase
    {
        protected const string TestPlant = "PCS$PlantA";
        protected readonly Project Project = new(TestPlant, ProjectName, $"Description of {ProjectName} project", ProjectGuid1);
        protected const string ProjectName = "Pname";
        protected static Guid ProjectGuid1 => new Guid("11111111-2222-2222-2222-333333333341");
        protected static Guid ProjectGuid2 => new Guid("11111111-2222-2222-2222-333333333342");
        protected const int ProjectId = 480;
        protected const string FilterName = "Fname";
        protected const string Criteria = "Fcriteria";
        protected SavedFilter _savedFilter;
        protected readonly Guid CurrentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
        protected DbContextOptions<IPOContext> _dbContextOptions;
        protected Mock<IPlantProvider> _plantProviderMock;
        protected IPlantProvider _plantProvider;
        protected ICurrentUserProvider _currentUserProvider;
        protected Mock<IPersonApiService> _personApiServiceMock;
        protected IPersonApiService _personApiService;
        protected IEventDispatcher _eventDispatcher;
        protected ManualTimeProvider _timeProvider;
        protected IPermissionCache _permissionCache;
        protected Mock<IPermissionCache> _permissionCacheMock;

        [TestInitialize]
        public void SetupBase()
        {
            Project.SetProtectedIdForTesting(ProjectId);

            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock.SetupGet(x => x.Plant).Returns(TestPlant);
            _plantProvider = _plantProviderMock.Object;
            
            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiService = _personApiServiceMock.Object;

            var currentUserProviderMock = new Mock<ICurrentUserProvider>();
            currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(CurrentUserOid);
            _currentUserProvider = currentUserProviderMock.Object;

            var eventDispatcher = new Mock<IEventDispatcher>();
            _eventDispatcher = eventDispatcher.Object;

            _permissionCacheMock = new Mock<IPermissionCache>();
            _permissionCacheMock.Setup(x => x.GetProjectsForUserAsync(TestPlant, CurrentUserOid))
                .Returns(Task.FromResult(new List<AccessableProject>
                {
                    new() {Name = "Project1"}, 
                    new() {Name = "Project2"}
                } as IList<AccessableProject>));
            _permissionCache = _permissionCacheMock.Object;

            _timeProvider = new ManualTimeProvider(new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc));
            TimeService.SetProvider(_timeProvider);

            _dbContextOptions = CreateDbContextOptions();

            // ensure current user exists in db
            using (var context = CreateDbContext(_dbContextOptions))// new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
               var createResult = context.Database.EnsureCreated();

                if (context.Persons.SingleOrDefault(p => p.Guid == CurrentUserOid) == null)
                {
                    var person = AddPerson(context, CurrentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs");
                    AddProject(context, Project);
                    AddSavedFiltersToPerson(context, person);
                }
            }

            SetupNewDatabase(_dbContextOptions);
        }

        protected abstract DbContextOptions<IPOContext> CreateDbContextOptions();

        protected abstract IPOContext CreateDbContext(DbContextOptions<IPOContext> dbContextOptions);

        protected abstract void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions);

        protected Project GetProjectById(int projectId)
        {
            using var context = CreateDbContext(_dbContextOptions);//  new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
            return context.Projects.Single(x => x.Id == projectId);
        }

        protected Person AddPerson(IPOContext context, Guid oid, string firstName, string lastName, string userName, string email)
        {
            var person = new Person(oid, firstName, lastName, userName, email);
            context.Persons.Add(person);
            context.SaveChangesAsync().Wait();
            return person;
        }

        protected Person AddSavedFiltersToPerson(IPOContext context, Person person)
        {
            _savedFilter = new SavedFilter(TestPlant, Project, FilterName, Criteria);
            var filter2 = new SavedFilter(TestPlant, Project, "filter2", Criteria);
            person.AddSavedFilter(_savedFilter);
            person.AddSavedFilter(filter2);
            //context.Projects.Add(Project);
            context.SaveChangesAsync().Wait();
            return person;
        }

        protected Project AddProject(IPOContext context, Project project)
        {
            context.Projects.Add(project);
            context.SaveChangesAsync().Wait();
            return project;
        }
    }
}
