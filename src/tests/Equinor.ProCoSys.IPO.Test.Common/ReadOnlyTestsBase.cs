using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public abstract class ReadOnlyTestsBase
    {
        protected const string TestPlant = "PCS$PlantA";
        protected readonly Project Project = new(TestPlant, ProjectName, $"Description of {ProjectName} project");
        protected const string ProjectName = "Pname";
        protected const int ProjectId = 480;
        protected const string FilterName = "Fname";
        protected const string Criteria = "Fcriteria";
        protected SavedFilter _savedFilter;
        protected readonly Guid _currentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
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
            currentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(_currentUserOid);
            _currentUserProvider = currentUserProviderMock.Object;

            var eventDispatcher = new Mock<IEventDispatcher>();
            _eventDispatcher = eventDispatcher.Object;

            _permissionCacheMock = new Mock<IPermissionCache>();
            _permissionCacheMock.Setup(x => x.GetProjectsForUserAsync(TestPlant, _currentUserOid))
                .Returns(Task.FromResult(new List<string> { "Project1", "Project2" } as IList<string>));
            _permissionCache = _permissionCacheMock.Object;

            _timeProvider = new ManualTimeProvider(new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc));
            TimeService.SetProvider(_timeProvider);

            _dbContextOptions = new DbContextOptionsBuilder<IPOContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            // ensure current user exists in db
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                if (context.Persons.SingleOrDefault(p => p.Oid == _currentUserOid) == null)
                {
                    var person = AddPerson(context, _currentUserOid, "Ole", "Lukkøye", "ol", "ol@pcs.pcs");
                    AddSavedFiltersToPerson(context, person);
                    AddProject(context, Project);
                }
            }

            SetupNewDatabase(_dbContextOptions);
        }

        protected abstract void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions);

        protected Project GetProjectById(int projectId)
        {
            using var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider);
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
