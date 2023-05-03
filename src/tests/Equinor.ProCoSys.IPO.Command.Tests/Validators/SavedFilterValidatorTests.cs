using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.Validators
{
    [TestClass]
    public class SavedFilterValidatorTests : ReadOnlyTestsBase
    {
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private Guid _personOid;
        private SavedFilterValidator _dut;
        private SavedFilter _savedFilter1;
        private SavedFilter _savedFilter2;

        private const string _title = "title";
        private const int _projectId = 320;
        private const string _projectName = "projectName";
        private readonly Project _project = new(TestPlant, _projectName, $"Description of {_projectName}");

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _project.SetProtectedIdForTesting(_projectId);

                _personOid = new Guid();

                var person = AddPerson(context, _personOid, "Current", "User", "", "");
                _savedFilter1 = new SavedFilter(TestPlant, _project, _title, "criteria");
                _savedFilter2 = new SavedFilter(TestPlant, _project, _title, "C");
                context.Projects.Add(_project);
                person.AddSavedFilter(_savedFilter1);
                person.AddSavedFilter(_savedFilter2);
                context.SaveChangesAsync().Wait();

                _currentUserProviderMock = new Mock<ICurrentUserProvider>();
                _currentUserProviderMock
                    .Setup(x => x.GetCurrentUserOid())
                    .Returns(_personOid);
            }
        }

        [TestMethod]
        public async Task ExistsWithSameTitleForPersonInProjectAsync_UnknownTitle_ShouldReturnFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsWithSameTitleForPersonInProjectOrAcrossAllProjectsAsync("xxx", _projectName, default);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ExistsWithSameTitleForPersonInProjectAsync_KnownTitle_ShouldReturnTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsWithSameTitleForPersonInProjectOrAcrossAllProjectsAsync(_title, _projectName, default);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ExistsWithSameTitleForPersonInProjectAsync_NonExistingProject_ShouldReturnFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                       _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsWithSameTitleForPersonInProjectOrAcrossAllProjectsAsync(_title, "NonExistingProject", default);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ExistsAnotherWithSameTitleForPersonInProjectAsync_NewTitle_ShouldReturnFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsAnotherWithSameTitleForPersonInProjectAsync(2, "xxx", default);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ExistsAnotherWithSameTitleForPersonInProjectAsync_SameTitleAsAnotherSavedFilter_ShouldReturnTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsAnotherWithSameTitleForPersonInProjectAsync(_savedFilter2.Id, _title, default);

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ExistsAnotherWithSameTitleForPersonInProjectAsync_NonExistingProject_ShouldReturnFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                       _currentUserProvider))
            {
                var savedFilter = await context.SavedFilters.FindAsync(2);
                savedFilter.ProjectId = 123123213;
                await context.SaveChangesAsync();

                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsAnotherWithSameTitleForPersonInProjectAsync(2, "xxx", default);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ExistsAsync_UnknownSavedFilter_ShouldReturnFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsAsync(99, default);

                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ExistsWithAsync_KnownSavedFilter_ShouldReturnTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _dut = new SavedFilterValidator(context, _currentUserProviderMock.Object);
                var result = await _dut.ExistsAsync(_savedFilter1.Id, default);

                Assert.IsTrue(result);
            }
        }
    }
}
