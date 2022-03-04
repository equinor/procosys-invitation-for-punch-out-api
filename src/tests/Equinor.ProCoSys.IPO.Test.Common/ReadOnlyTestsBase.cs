using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public abstract class ReadOnlyTestsBase
    {
        protected const string TestPlant = "PCS$PlantA";
        protected const string ProjectName = "Pname";
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

        [TestInitialize]
        public void SetupBase()
        {
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
                }
            }

            SetupNewDatabase(_dbContextOptions);
        }

        protected abstract void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions);

        protected Person AddPerson(IPOContext context, Guid oid, string firstName, string lastName, string userName, string email)
        {
            var person = new Person(oid, firstName, lastName, userName, email);
            context.Persons.Add(person);
            context.SaveChangesAsync().Wait();
            return person;
        }

        protected Person AddSavedFiltersToPerson(IPOContext context, Person person)
        {
            _savedFilter = new SavedFilter(TestPlant, ProjectName, FilterName, Criteria);
            var filter2 = new SavedFilter(TestPlant, ProjectName, "filter2", Criteria);
            person.AddSavedFilter(_savedFilter);
            person.AddSavedFilter(filter2);
            context.SaveChangesAsync().Wait();
            return person;
        }
    }
}
