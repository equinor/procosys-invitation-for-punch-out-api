using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Domain.Events;
using Equinor.ProCoSys.Auth.Time;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Equinor.ProCoSys.Auth.Misc;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests
{
    [TestClass]
    public class UnitOfWorkTests
    {
        private const string Plant = "PCS$TESTPLANT";
        private readonly Project project = new(Plant, "Project", "Description of Project");
        private readonly Guid _currentUserOid = new Guid("12345678-1234-1234-1234-123456789123");
        private readonly DateTime _currentTime = new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        private McPkg _mcPkg;

        private DbContextOptions<IPOContext> _dbContextOptions;
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IEventDispatcher> _eventDispatcherMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private ManualTimeProvider _timeProvider;

        [TestInitialize]
        public void Setup()
        {
            _mcPkg = new McPkg(Plant, project, "commno", "mcno", "d", "1|2");

            _dbContextOptions = new DbContextOptionsBuilder<IPOContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock.Setup(x => x.Plant)
                .Returns(Plant);

            _eventDispatcherMock = new Mock<IEventDispatcher>();

            _currentUserProviderMock = new Mock<ICurrentUserProvider>();

            _timeProvider = new ManualTimeProvider(new DateTime(2020, 2, 1, 0, 0, 0, DateTimeKind.Utc));
            TimeService.SetProvider(_timeProvider);
        }

        [TestMethod]
        public async Task SaveChangesAsync_SetsCreatedProperties_WhenCreated()
        {
            using var dut = new IPOContext(_dbContextOptions, _plantProviderMock.Object, _eventDispatcherMock.Object, _currentUserProviderMock.Object);

            var user = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs");
            dut.Persons.Add(user);
            dut.SaveChanges();

            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid())
                .Returns(_currentUserOid);
            var newInvitation = new Invitation(Plant, project, "Title", "Desc", DisciplineType.DP,
                _currentTime.AddDays(1), _currentTime.AddDays(2), "Loc", new List<McPkg> {_mcPkg}, null);
            dut.Invitations.Add(newInvitation);

            await dut.SaveChangesAsync();

            Assert.AreEqual(_currentTime, newInvitation.CreatedAtUtc);
            Assert.AreEqual(user.Id, newInvitation.CreatedById);
            Assert.IsNull(newInvitation.ModifiedAtUtc);
            Assert.IsNull(newInvitation.ModifiedById);
        }

        [TestMethod]
        public async Task SaveChangesAsync_SetsModifiedProperties_WhenModified()
        {
            using var dut = new IPOContext(_dbContextOptions, _plantProviderMock.Object, _eventDispatcherMock.Object, _currentUserProviderMock.Object);

            var user = new Person(_currentUserOid, "Current", "User", "cu", "cu@pcs.pcs");
            dut.Persons.Add(user);
            dut.SaveChanges();

            _currentUserProviderMock
                .Setup(x => x.GetCurrentUserOid())
                .Returns(_currentUserOid);

            var newInvitation = new Invitation(Plant, project, "Title", "Desc", DisciplineType.DP,
                _currentTime.AddDays(1), _currentTime.AddDays(2), "Loc", new List<McPkg> {_mcPkg}, null);
            dut.Invitations.Add(newInvitation);

            await dut.SaveChangesAsync();

            newInvitation.Title = "UpdatedTitle";
            await dut.SaveChangesAsync();

            Assert.AreEqual(_currentTime, newInvitation.ModifiedAtUtc);
            Assert.AreEqual(user.Id, newInvitation.ModifiedById);
        }
    }
}
