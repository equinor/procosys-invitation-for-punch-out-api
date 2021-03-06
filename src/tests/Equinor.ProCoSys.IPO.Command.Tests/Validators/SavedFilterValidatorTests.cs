﻿using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
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

        private readonly string _title = "title";
        private readonly string _projectName = "projectName";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher,
                _currentUserProvider))
            {
                _personOid = new Guid();

                var person = AddPerson(context, _personOid, "Current", "User", "", "");
                _savedFilter1 = new SavedFilter(TestPlant, _projectName, _title, "criteria");
                _savedFilter2 = new SavedFilter(TestPlant, _projectName, _title, "C");

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
                var result = await _dut.ExistsWithSameTitleForPersonInProjectAsync("xxx", _projectName, default);

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
                var result = await _dut.ExistsWithSameTitleForPersonInProjectAsync(_title, _projectName, default);

                Assert.IsTrue(result);
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
