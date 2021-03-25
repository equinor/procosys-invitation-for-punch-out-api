using System;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.PersonCommands.CreateSavedFilter
{
    [TestClass]
    public class CreateSavedFilterCommandHandlerTests : CommandHandlerTestsBase
    {
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IMainProjectApiService> _projectApiServiceMock;
        private Person _person;
        private CreateSavedFilterCommand _command;
        private CreateSavedFilterCommandHandler _dut;

        private const string _title = "T1";
        private const string _criteria = "C1";
        private const string _projectName = "Project";
        private readonly Guid _currentUserOid = new Guid("12345678-1234-1234-1234-123456789123");

        [TestInitialize]
        public void Setup()
        {
            // Arrange
            _person = new Person(_currentUserOid, "Current", "User", "", "");
            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock
                .Setup(p => p.GetWithSavedFiltersByOidAsync(It.Is<Guid>(x => x == CurrentUserOid)))
                .Returns(Task.FromResult(_person));

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid())
                .Returns(CurrentUserOid);

            var project = new ProCoSysProject
            {
                Description = "Description", Id = 1, IsClosed = false, Name = _projectName
            };

            _projectApiServiceMock = new Mock<IMainProjectApiService>();
            _projectApiServiceMock
                .Setup(x => x.TryGetProjectAsync(TestPlant, _projectName))
                .Returns(Task.FromResult(project));

            _command = new CreateSavedFilterCommand(_projectName, _title, _criteria, true);

            _dut = new CreateSavedFilterCommandHandler(
                _personRepositoryMock.Object,
                UnitOfWorkMock.Object,
                PlantProviderMock.Object,
                _currentUserProviderMock.Object,
                _projectApiServiceMock.Object);
        }

        [TestMethod]
        public async Task HandlingCreateSavedFilterCommand_ShouldAddSavedFilterToPerson()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            var savedFilter = _person.SavedFilters.Single();
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Data);
            Assert.IsTrue(savedFilter.DefaultFilter);
            Assert.AreEqual(_projectName, savedFilter.ProjectName);
            Assert.AreEqual(_title, savedFilter.Title);
            Assert.AreEqual(_criteria, savedFilter.Criteria);
        }

        [TestMethod]
        public async Task HandlingCreateSavedFilterCommand_ShouldOverrideDefaultFilter()
        {
            await _dut.Handle(_command, default);
            Assert.AreEqual(1, _person.SavedFilters.Count);

            // Act
            _command = new CreateSavedFilterCommand(_projectName, "T2", "C2", true);

            // Assert
            var result = await _dut.Handle(_command, default);
            var savedFilter = _person.SavedFilters.First();
            var addedSavedFilter = _person.SavedFilters.Last();
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(0, result.Data);
            Assert.IsFalse(savedFilter.DefaultFilter);
            Assert.IsTrue(addedSavedFilter.DefaultFilter);
        }

        [TestMethod]
        public async Task HandlingCreateSavedFilterCommand_ShouldSave()
        {
            // Act
            await _dut.Handle(_command, default);

            // Assert
            UnitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}
