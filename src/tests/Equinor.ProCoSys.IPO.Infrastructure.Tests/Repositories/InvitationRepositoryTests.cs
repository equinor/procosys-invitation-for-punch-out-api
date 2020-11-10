using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.Moq;
using Moq;

namespace Equinor.ProCoSys.IPO.Infrastructure.Tests.Repositories
{
    [TestClass]
    public class InvitationRepositoryTests : RepositoryTestBase
    {
        private const int InvitationId = 5;
        private const int McPkgId = 51;
        private const int ParticipantId = 1;
        private List<Invitation> _invitations;
        private Mock<DbSet<Invitation>> _dbSetMock;

        private InvitationRepository _dut;
        private McPkg _mcPkg;
        private Participant _participant;
        private Invitation _invitation;

        [TestInitialize]
        public void Setup()
        {
            var mcPkgMock = new Mock<McPkg>();
            mcPkgMock.SetupGet(m => m.Plant).Returns(TestPlant);

            var responsibleMock = new Mock<Participant>();
            responsibleMock.SetupGet(x => x.Plant).Returns(TestPlant);

            _mcPkg = new McPkg(TestPlant, "ProjectName", "Comm1", "MC1", "Description");
            _mcPkg.SetProtectedIdForTesting(McPkgId);

            _participant = new Participant(TestPlant, Organization.Contractor, IpoParticipantType.FunctionalRole, "FR", null, null, "fr@test.com", null, 0);
            _participant.SetProtectedIdForTesting(ParticipantId);

            _invitation = new Invitation(TestPlant, "ProjectName", "Title", "D", DisciplineType.DP);
            _invitation.SetProtectedIdForTesting(InvitationId);

            _invitation.AddMcPkg(_mcPkg);
            _invitation.AddParticipant(_participant);

            _invitations = new List<Invitation>
            {
                _invitation
            };

            _dbSetMock = _invitations.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.Invitations)
                .Returns(_dbSetMock.Object);

            _dut = new InvitationRepository(ContextHelper.ContextMock.Object);
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
            var result = await _dut.Exists(InvitationId);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task Exists_UnknownId_ShouldReturnFalse()
        {
            var result = await _dut.Exists(1234);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetById_KnownId_ShouldReturnInvitation()
        {
            var result = await _dut.GetByIdAsync(InvitationId);

            Assert.AreEqual(InvitationId, result.Id);
        }

        [TestMethod]
        public async Task GetById_UnknownId_ShouldReturnNull()
        {
            var result = await _dut.GetByIdAsync(1234);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Add_Invitation_ShouldCallAddForInvitation()
        {
            _dut.Add(_invitation);

            _dbSetMock.Verify(s => s.Add(_invitation), Times.Once);
        }
    }
}
