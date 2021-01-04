using System;
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
        private const int InvitationWithMcPkgId = 5;
        private const int McPkgId = 51;
        private const int CommPkgId = 71;
        private const int ParticipantId = 1;
        private List<Invitation> _invitations;
        private Mock<DbSet<Invitation>> _dbSetMock;

        private InvitationRepository _dut;
        private McPkg _mcPkg;
        private CommPkg _commPkg;
        private Participant _participant;
        private Attachment _attachment;
        private Invitation _invitationWithMcPkg;
        private Invitation _invitationWithCommPkg;

        [TestInitialize]
        public void Setup()
        {
            var mcPkgMock = new Mock<McPkg>();
            mcPkgMock.SetupGet(m => m.Plant).Returns(TestPlant);

            var participantMock = new Mock<Participant>();
            participantMock.SetupGet(x => x.Plant).Returns(TestPlant);

            _mcPkg = new McPkg(TestPlant, "ProjectName", "Comm1", "MC1", "Description");
            _mcPkg.SetProtectedIdForTesting(McPkgId);

            _commPkg = new CommPkg(TestPlant, "ProjectName", "Comm1", "Description", "OK");
            _commPkg.SetProtectedIdForTesting(CommPkgId);

            _participant = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                "FR",
                null,
                null,
                null,
                "fr@test.com",
                null,
                0);
            _participant.SetProtectedIdForTesting(ParticipantId);

            _invitationWithMcPkg = new Invitation(
                TestPlant,
                "ProjectName",
                "Title",
                "D",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null);
            _invitationWithMcPkg.SetProtectedIdForTesting(InvitationWithMcPkgId);
            _invitationWithCommPkg = new Invitation(
                TestPlant,
                "ProjectName",
                "Title 2",
                "D",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                null);

            _attachment = new Attachment(TestPlant, "filename.txt");

            _invitationWithMcPkg.AddMcPkg(_mcPkg);
            _invitationWithMcPkg.AddParticipant(_participant);
            _invitationWithMcPkg.AddAttachment(_attachment);
            _invitationWithCommPkg.AddCommPkg(_commPkg);

            _invitations = new List<Invitation>
            {
                _invitationWithMcPkg,
                _invitationWithCommPkg
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

            Assert.AreEqual(2, result.Count);
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
            var result = await _dut.Exists(InvitationWithMcPkgId);

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
            var result = await _dut.GetByIdAsync(InvitationWithMcPkgId);

            Assert.AreEqual(InvitationWithMcPkgId, result.Id);
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
            _dut.Add(_invitationWithMcPkg);

            _dbSetMock.Verify(x => x.Add(_invitationWithMcPkg), Times.Once);
        }

        [TestMethod]
        public void RemoveMcPkg_KnownMcPkg_ShouldRemoveMcPkg()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.McPkgs.Count);

            _invitationWithMcPkg.RemoveMcPkg(_mcPkg);

            Assert.AreEqual(0, _invitationWithMcPkg.McPkgs.Count);
        }

        [TestMethod]
        public void RemoveMcPkg_UnknownMcPkg_ShouldNotRemoveMcPkg()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.McPkgs.Count);

            _invitationWithMcPkg.RemoveMcPkg(new McPkg(TestPlant, "Project name", "Comm1", "MC 02", "D"));

            Assert.AreEqual(1, _invitationWithMcPkg.McPkgs.Count);
        }

        [TestMethod]
        public void RemoveParticipant_KnownParticipant_ShouldRemoveParticipant()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.Participants.Count);

            _invitationWithMcPkg.RemoveParticipant(_participant);

            Assert.AreEqual(0, _invitationWithMcPkg.Participants.Count);
        }

        [TestMethod]
        public void RemoveParticipant_UnknownParticipant_ShouldNotRemoveParticipant()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.Participants.Count);

            _invitationWithMcPkg.RemoveParticipant(new Participant(
                TestPlant,
                Organization.Operation,
                IpoParticipantType.FunctionalRole,
                "FR 2",
                null,
                null,
                null,
                "fr@test.com",
                null,
                2));

            Assert.AreEqual(1, _invitationWithMcPkg.Participants.Count);
        }

        [TestMethod]
        public void RemoveCommPkg_KnownCommPkg_ShouldRemoveCommPkg()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.CommPkgs.Count);

            _invitationWithCommPkg.RemoveCommPkg(_commPkg);

            Assert.AreEqual(0, _invitationWithCommPkg.CommPkgs.Count);
        }

        [TestMethod]
        public void RemoveCommPkg_UnknownCommPkg_ShouldNotRemoveCommPkg()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.CommPkgs.Count);

            _invitationWithCommPkg.RemoveCommPkg(new CommPkg(TestPlant, "Project name", "Comm2", "D", "PA"));

            Assert.AreEqual(1, _invitationWithCommPkg.CommPkgs.Count);
        }

        [TestMethod]
        public void RemoveAttachment_KnownAttachment_ShouldRemoveAttachment()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.Attachments.Count);

            _invitationWithMcPkg.RemoveAttachment(_attachment);

            Assert.AreEqual(0, _invitationWithMcPkg.Attachments.Count);
        }

        [TestMethod]
        public void RemoveAttachment_UnknownAttachment_ShouldNotRemoveAttachment()
        {
            Assert.AreEqual(1, _invitationWithMcPkg.Attachments.Count);

            _invitationWithMcPkg.RemoveAttachment(new Attachment(TestPlant, "unknown.txt"));

            Assert.AreEqual(1, _invitationWithMcPkg.Attachments.Count);
        }
    }
}
