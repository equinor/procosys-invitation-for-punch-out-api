﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.UpdateSavedFilter;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Project;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.PersonCommands.UpdateSavedFilter
{
    [TestClass]
    public class UpdateSavedFilterCommandHandlerTests : CommandHandlerTestsBase
    {
        private readonly string _newTitle = "NewSavedFilterTitle";
        private readonly string _oldTitle = "OldSavedFilterTitle";
        private readonly string _newCriteria = "NewSavedFilterCriteria";
        private readonly string _oldCriteria = "OldSavedFilterCriteria";
        private bool _newDefaultFilter = true;
        private readonly string _rowVersion = "AAAAAAAAABA=";
        private readonly Guid _currentUserOid = new Guid();
        private readonly int _projectId = 320;
        private readonly Project _project = new("PCS$TEST_PLANT", $"Project", $"Description of Project");

        private UpdateSavedFilterCommand _command;
        private UpdateSavedFilterCommandHandler _dut;

        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IProjectRepository> _projectRepositoryMock;

        //private ProCoSysProject _project;
        private Person _person;
        private SavedFilter _savedFilter;

        [TestInitialize]
        public void Setup()
        {
            _project.SetProtectedIdForTesting(_projectId);
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectRepositoryMock.Setup(x => x.GetByIdAsync(_projectId)).Returns(Task.FromResult(_project));

            _person = new Person(_currentUserOid, "FirstName", "LastName", "UserName" ,"email@address.com");
            //_project = new ProCoSysProject() { Id = 0, Description = "", IsClosed = false, Name = "ProjectName" };

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();
            _currentUserProviderMock.Setup(x => x.GetCurrentUserOid())
                .Returns(_currentUserOid);

            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepositoryMock.Setup(x => x.GetWithSavedFiltersByOidAsync(_currentUserOid))
                .Returns(Task.FromResult(_person));

            _savedFilter = new SavedFilter(TestPlant, _project, _oldTitle, _oldCriteria);
            _savedFilter.SetProtectedIdForTesting(2);
            _person.AddSavedFilter(_savedFilter);

            _command = new UpdateSavedFilterCommand(_savedFilter.Id, _newTitle, _newCriteria, _newDefaultFilter, _rowVersion);

            _dut = new UpdateSavedFilterCommandHandler(
                UnitOfWorkMock.Object,
                _currentUserProviderMock.Object,
                _personRepositoryMock.Object,
                _projectRepositoryMock.Object
            );
        }

        [TestMethod]
        public async Task HandlingUpdateSavedFilterCommand_ShouldUpdateSavedFilter()
        {
            // Arrange
            Assert.AreEqual(_oldTitle, _savedFilter.Title);
            Assert.AreEqual(_oldCriteria, _savedFilter.Criteria);
            Assert.AreEqual(false, _savedFilter.DefaultFilter);

            // Act
            await _dut.Handle(_command, default);

            // Arrange
            Assert.AreEqual(_newTitle, _savedFilter.Title);
            Assert.AreEqual(_newCriteria, _savedFilter.Criteria);
            Assert.AreEqual(_newDefaultFilter, _savedFilter.DefaultFilter);
        }

        [TestMethod]
        public async Task HandlingUpdateSavedFilterCommand_ShouldNotUpdateDefaultFilter_IfDefaultFilterIsNull()
        {
            // Arrange
            _command = new UpdateSavedFilterCommand(_savedFilter.Id, _newTitle, _newCriteria, null, _rowVersion);

            // Act
            await _dut.Handle(_command, default);

            // Arrange
            Assert.AreEqual(_newTitle, _savedFilter.Title);
            Assert.AreEqual(_newCriteria, _savedFilter.Criteria);
            Assert.AreEqual(false, _savedFilter.DefaultFilter);
        }

        [TestMethod]
        public async Task HandlingUpdateSavedFilterCommand_ShouldSetAndReturnRowVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            Assert.AreEqual(0, result.Errors.Count);
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.AreEqual(_rowVersion, result.Data);
            Assert.AreEqual(_rowVersion, _savedFilter.RowVersion.ConvertToString());
        }

        [TestMethod]
        public async Task HandlingUpdateSavedFilterCommand_ShouldSave()
        {
            await _dut.Handle(_command, default);
            UnitOfWorkMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
        }
    }
}
