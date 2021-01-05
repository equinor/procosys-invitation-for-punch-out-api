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
        private Mock<DbSet<Invitation>> _dbInvitationSetMock;
        private Mock<DbSet<Attachment>> _attachmentSetMock;
        private Mock<DbSet<Participant>> _participantSetMock;
        private Mock<DbSet<McPkg>> _mcPkgSetMock;
        private Mock<DbSet<CommPkg>> _commPkgSetMock;

        private InvitationRepository _dut;
        private McPkg _mcPkg;
        private CommPkg _commPkg;
        private Participant _participant;
        private Attachment _attachment;
        private Comment _comment;
        private Invitation _invitationWithMcPkg;
        private Invitation _invitationWithCommPkg;

        [TestInitialize]
        public void Setup()
        {
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

            _comment = new Comment(TestPlant, "comment");
            _invitationWithCommPkg.AddComment(_comment);

            _invitations = new List<Invitation>
            {
                _invitationWithMcPkg,
                _invitationWithCommPkg
            };

            _dbInvitationSetMock = _invitations.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.Invitations)
                .Returns(_dbInvitationSetMock.Object);

            var attachments = new List<Attachment>
            {
                _attachment
            };

            _attachmentSetMock = attachments.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.Attachments)
                .Returns(_attachmentSetMock.Object);

            var participants = new List<Participant>
            {
                _participant
            };

            _participantSetMock = participants.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.Participants)
                .Returns(_participantSetMock.Object);

            var mcPkgs = new List<McPkg>
            {
                _mcPkg
            };

            _mcPkgSetMock = mcPkgs.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.McPkgs)
                .Returns(_mcPkgSetMock.Object);

            var commPkgs = new List<CommPkg>
            {
                _commPkg
            };

            _commPkgSetMock = commPkgs.AsQueryable().BuildMockDbSet();

            ContextHelper
                .ContextMock
                .Setup(x => x.CommPkgs)
                .Returns(_commPkgSetMock.Object);

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

            _dbInvitationSetMock.Verify(x => x.Add(_invitationWithMcPkg), Times.Once);
        }

        [TestMethod]
        public void RemoveMcPkg_KnownMcPkg_ShouldRemoveMcPkg()
        {
            _dut.RemoveMcPkg(_mcPkg);

            _mcPkgSetMock.Verify(s => s.Remove(_mcPkg), Times.Once);
        }

        [TestMethod]
        public void RemoveMcPkg_UnknownMcPkg_ShouldNotRemoveMcPkg()
        {
            _dut.RemoveMcPkg(new McPkg(TestPlant, "Project name", "Comm1", "MC 02", "D"));

            _mcPkgSetMock.Verify(s => s.Remove(_mcPkg), Times.Never);
        }

        [TestMethod]
        public void RemoveParticipant_KnownParticipant_ShouldRemoveParticipant()
        {
            _dut.RemoveParticipant(_participant);

            _participantSetMock.Verify(s => s.Remove(_participant), Times.Once);
        }

        [TestMethod]
        public void RemoveParticipant_UnknownParticipant_ShouldNotRemoveParticipant()
        {
            _dut.RemoveParticipant(new Participant(
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

            _participantSetMock.Verify(s => s.Remove(_participant), Times.Never);
        }

        [TestMethod]
        public void RemoveCommPkg_KnownCommPkg_ShouldRemoveCommPkg()
        {
            _dut.RemoveCommPkg(_commPkg);

            _commPkgSetMock.Verify(s => s.Remove(_commPkg), Times.Once);
        }

        [TestMethod]
        public void RemoveCommPkg_UnknownCommPkg_ShouldNotRemoveCommPkg()
        {
            _dut.RemoveCommPkg(new CommPkg(TestPlant, "Project name", "Comm2", "D", "PA"));

            _commPkgSetMock.Verify(s => s.Remove(_commPkg), Times.Never);
        }

        [TestMethod]
        public void RemoveComment_KnownComment_ShouldRemoveComment()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.Comments.Count);

            _invitationWithCommPkg.RemoveComment(_comment);

            Assert.AreEqual(0, _invitationWithCommPkg.Comments.Count);
        }

        [TestMethod]
        public void RemoveComment_UnknownComment_ShouldNotRemoveComment()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.Comments.Count);

            _invitationWithCommPkg.RemoveComment(new Comment(TestPlant, "comment does not exist"));

            Assert.AreEqual(1, _invitationWithCommPkg.CommPkgs.Count);
        }

        [TestMethod]
        public void RemoveAttachment_KnownAttachment_ShouldRemoveAttachment()
        {
            _dut.RemoveAttachment(_attachment);

            _attachmentSetMock.Verify(s => s.Remove(_attachment), Times.Once);
        }


        [TestMethod]
        public void RemoveAttachment_UnknownAttachment_ShouldNotRemoveAttachment()
        {
            _dut.RemoveAttachment(new Attachment(TestPlant, "unknown.txt"));

            _attachmentSetMock.Verify(s => s.Remove(_attachment), Times.Never);
        }

        [TestMethod]
        public void RemoveComment_KnownComment_ShouldRemoveComment()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.Comments.Count);

            _invitationWithCommPkg.RemoveComment(_comment);

            Assert.AreEqual(0, _invitationWithCommPkg.Comments.Count);
        }

        [TestMethod]
        public void RemoveComment_UnknownComment_ShouldNotRemoveComment()
        {
            Assert.AreEqual(1, _invitationWithCommPkg.Comments.Count);

            _invitationWithCommPkg.RemoveComment(new Comment(TestPlant, "comment does not exist"));

            Assert.AreEqual(1, _invitationWithCommPkg.CommPkgs.Count);
        }
    }
}
