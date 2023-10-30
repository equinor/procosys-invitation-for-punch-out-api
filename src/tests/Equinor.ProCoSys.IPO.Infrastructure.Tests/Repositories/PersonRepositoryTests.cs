using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;
using Moq;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests.Repositories
{
    [TestClass]
    public class PersonRepositoryTests : RepositoryTestBase
    {
        private const int PersonId = 5;
        private const int SavedFilterId = 51;
        private const string _projectName = "ProjectName";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project project = new("PCS$TEST_PLANT", _projectName, $"Description of {_projectName} project", _projectGuid);
        private Guid Oid = new Guid("11111111-1111-2222-2222-333333333333");
        private List<Person> _persons;
        private Mock<DbSet<Person>> _dbPersonSetMock;
        private Mock<DbSet<SavedFilter>> _savedFilterSetMock;

        private PersonRepository _dut;
        private Person _person;
        private SavedFilter _savedFilter;

        [TestInitialize]
        public void Setup()
        {
            _person = new Person(
                Oid,
                "FirstName",
                "LastName",
                "UNAME",
                "email@test.com");
            _person.SetProtectedIdForTesting(PersonId);

            _savedFilter = new SavedFilter(TestPlant, project, "title", "criteria");
            _savedFilter.SetProtectedIdForTesting(SavedFilterId);
            _person.AddSavedFilter(_savedFilter);

            _persons = new List<Person>
            {
                _person
            };

            _dbPersonSetMock = _persons.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.Persons)
                .Returns(_dbPersonSetMock.Object);

            var savedFilters = new List<SavedFilter>
            {
                _savedFilter
            };

            _savedFilterSetMock = savedFilters.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.SavedFilters)
                .Returns(_savedFilterSetMock.Object);

            _dut = new PersonRepository(ContextHelper.ContextMock.Object);
        }

        [TestMethod]
        public async Task GetAll_ShouldReturnAllItems()
        {
            var result = await _dut.GetAllAsync();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public async Task GetByIds_UnknownId_ShouldReturnEmptyList()
        {
            var result = await _dut.GetByIdsAsync(new List<int> { 1234 });

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task Exists_KnownId_ShouldReturnTrue()
        {
            var result = await _dut.Exists(PersonId);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Exists_UnknownId_ShouldReturnFalse()
        {
            var result = await _dut.Exists(1234);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetById_KnownId_ShouldReturnPerson()
        {
            var result = await _dut.GetByIdAsync(PersonId);

            Assert.AreEqual(PersonId, result.Id);
        }

        [TestMethod]
        public async Task GetById_UnknownId_ShouldReturnNull()
        {
            var result = await _dut.GetByIdAsync(1234);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetWithSavedFiltersByOidAsync_KnownId_ShouldReturnPerson()
        {
            var result = await _dut.GetWithSavedFiltersByOidAsync(Oid);

            Assert.AreEqual(PersonId, result.Id);
        }

        [TestMethod]
        public async Task GetWithSavedFiltersByOidAsync_UnknownId_ShouldReturnNull()
        {
            var result = await _dut.GetWithSavedFiltersByOidAsync(new Guid("11111111-1111-2222-2222-333333333331"));

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_Invitation_ShouldCallAddForPerson()
        {
            _dut.Add(_person);

            _dbPersonSetMock.Verify(x => x.Add(_person), Times.Once);
        }

        [TestMethod]
        public void RemoveSavedFilter_KnownSavedFilter_ShouldRemoveSavedFilter()
        {
            _dut.RemoveSavedFilter(_savedFilter);

            _savedFilterSetMock.Verify(s => s.Remove(_savedFilter), Times.Once);
        }
    }
}
