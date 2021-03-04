using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.Events.PreSave;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Domain.Tests.AggregateModels.InvitationAggregate
{
    [TestClass]
    public class InvitationTests
    {
        private Invitation _dutWithMcPkgScope;
        private Invitation _dutWithCommPkgScope;
        private Invitation _dutWithCanceledStatus;
        private Participant _personParticipant;
        private Participant _personParticipant2;
        private Participant _functionalRoleParticipant;
        private Participant _externalParticipant;
        private Person _currentPerson;
        private int _currentUserId = 99;
        private int _personParticipantId;
        private int _functionalRoleParticipantId;
        private int _externalParticipantId;
        private McPkg _mcPkg1;
        private McPkg _mcPkg2;
        private CommPkg _commPkg1;
        private CommPkg _commPkg2;
        private Comment _comment;
        private Attachment _attachment;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private const string Title = "Title A";
        private const string Title2 = "Title B";
        private const string Description = "Description A";
        private const string System = "12";
        private const string ParticipantRowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));

            _dutWithMcPkgScope = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            _dutWithCommPkgScope = new Invitation(
                TestPlant,
                ProjectName,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null);

            _dutWithCanceledStatus = new Invitation(
                TestPlant,
                ProjectName,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null);
            _personParticipantId = 10033;
            _functionalRoleParticipantId = 3;
            _externalParticipantId = 967;

            _mcPkg1 = new McPkg(TestPlant, ProjectName, "Comm1", "Mc1", "MC D", System);
            _mcPkg2 = new McPkg(TestPlant, ProjectName, "Comm1", "Mc2", "MC D 2", System);
            _commPkg1 = new CommPkg(TestPlant, ProjectName, "Comm1", "Comm D", "OK", "1|2");
            _commPkg2 = new CommPkg(TestPlant, ProjectName, "Comm2", "Comm D 2", "OK", "1|2");
            _comment = new Comment(TestPlant, "Comment text");
            _dutWithCommPkgScope.AddComment(_comment);
            _personParticipant = new Participant(
                TestPlant,
                Organization.Contractor,
                IpoParticipantType.Person,
                null,
                "Ola",
                "Nordmann",
                "ON",
                "ola@test.com",
                new Guid("11111111-1111-2222-2222-333333333333"),
                0);
            _personParticipant.SetProtectedIdForTesting(_personParticipantId);
            _functionalRoleParticipant = new Participant(
                TestPlant,
                Organization.ConstructionCompany,
                IpoParticipantType.FunctionalRole,
                "FR1",
                null,
                null,
                null,
                "fr1@test.com",
                null,
                1);
            _functionalRoleParticipant.SetProtectedIdForTesting(_functionalRoleParticipantId);
            _externalParticipant = new Participant(
                TestPlant,
                Organization.External,
                IpoParticipantType.Person,
                null,
                null,
                null,
                null,
                "external@ext.com",
                null,
                2);
            _externalParticipant.SetProtectedIdForTesting(_externalParticipantId);
            _personParticipant2 = new Participant(
                TestPlant, 
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                "Kari",
                "Hansen",
                "KH",
                "kari@test.com",
                new Guid("11111111-1111-2222-2222-333333333334"),
                0);

            _attachment = new Attachment(TestPlant, "filename.txt");
            _currentPerson = new Person(new Guid(), null, null, null, null);
            _currentPerson.SetProtectedIdForTesting(_currentUserId);

            _dutWithMcPkgScope.AddParticipant(_personParticipant);
            _dutWithMcPkgScope.AddParticipant(_functionalRoleParticipant);
            _dutWithMcPkgScope.AddParticipant(_externalParticipant);
            _dutWithMcPkgScope.AddParticipant(_personParticipant2);
            _dutWithMcPkgScope.AddMcPkg(_mcPkg1);
            _dutWithMcPkgScope.AddMcPkg(_mcPkg2);
            _dutWithMcPkgScope.AddAttachment(_attachment);
            _dutWithCommPkgScope.AddCommPkg(_commPkg1);
            _dutWithCommPkgScope.AddCommPkg(_commPkg2);
            _dutWithCommPkgScope.AddParticipant(_personParticipant);
            _dutWithCommPkgScope.AddParticipant(_functionalRoleParticipant);
            _dutWithCommPkgScope.CompleteIpo(
                _personParticipant,
                _personParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());
            _dutWithCanceledStatus.SetCreated(_currentPerson);
            _dutWithCanceledStatus.CancelIpo(_currentPerson);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dutWithMcPkgScope.Plant);
            Assert.AreEqual(ProjectName, _dutWithMcPkgScope.ProjectName);
            Assert.AreEqual(Title, _dutWithMcPkgScope.Title);
            Assert.AreEqual(Description, _dutWithMcPkgScope.Description);
            Assert.AreEqual(DisciplineType.MDP, _dutWithMcPkgScope.Type);
            Assert.AreEqual(4, _dutWithMcPkgScope.Participants.Count);
            Assert.AreEqual(2, _dutWithMcPkgScope.McPkgs.Count);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenTitleNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Invitation(
                    TestPlant,
                    ProjectName,
                    null,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Invitation(
                    TestPlant,
                    null,
                    Title,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null)
            );

        [TestMethod]
        public void AddMcPkg_ShouldThrowException_WhenMcPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.AddMcPkg(null));

        [TestMethod]
        public void AddCommPkg_ShouldThrowException_WhenCommPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.AddCommPkg(null));

        [TestMethod]
        public void AddParticipant_ShouldThrowException_WhenParticipantNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.AddParticipant(null));

        [TestMethod]
        public void AddAttachment_ShouldThrowException_WhenAttachmentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.AddAttachment(null));

        [TestMethod]
        public void AddComment_ShouldThrowException_WhenCommentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.AddComment(null));

        [TestMethod]
        public void RemoveParticipant_ShouldThrowException_WhenParticipantNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveParticipant(null));

        [TestMethod]
        public void RemoveAttachment_ShouldThrowException_WhenAttachmentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveAttachment(null));

        [TestMethod]
        public void RemoveMcPkg_ShouldThrowException_WhenMcPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveMcPkg(null));

        [TestMethod]
        public void RemoveCommPkg_ShouldThrowException_WhenCommPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveCommPkg(null));

        [TestMethod]
        public void RemoveComment_ShouldThrowException_WhenCommentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveComment(null));

        [TestMethod]
        public void AddMcPkg_ShouldAddMcPkgToMcPkgList()
        {
            var mcPkg = new Mock<McPkg>();
            mcPkg.SetupGet(mc => mc.Plant).Returns(TestPlant);

            _dutWithMcPkgScope.AddMcPkg(mcPkg.Object);

            Assert.AreEqual(3, _dutWithMcPkgScope.McPkgs.Count);
            Assert.IsTrue(_dutWithMcPkgScope.McPkgs.Contains(mcPkg.Object));
        }

        [TestMethod]
        public void RemoveMcPkg_ShouldRemoveMcPkgFromMcPkgList()
        {
            // Arrange
            Assert.AreEqual(2, _dutWithMcPkgScope.McPkgs.Count);

            // Act
            _dutWithMcPkgScope.RemoveMcPkg(_mcPkg1);

            // Assert
            Assert.AreEqual(1, _dutWithMcPkgScope.McPkgs.Count);
            Assert.IsFalse(_dutWithMcPkgScope.McPkgs.Contains(_mcPkg1));
        }

        [TestMethod]
        public void AddCommPkg_ShouldAddCommPkgToCommPkgList()
        {
            var commPkg = new Mock<CommPkg>();
            commPkg.SetupGet(mc => mc.Plant).Returns(TestPlant);

            _dutWithCommPkgScope.AddCommPkg(commPkg.Object);

            Assert.AreEqual(3, _dutWithCommPkgScope.CommPkgs.Count);
            Assert.IsTrue(_dutWithCommPkgScope.CommPkgs.Contains(commPkg.Object));
        }

        [TestMethod]
        public void RemoveCommPkg_ShouldRemoveCommPkgFromCommPkgList()
        {
            // Arrange
            Assert.AreEqual(2, _dutWithCommPkgScope.CommPkgs.Count);

            // Act
            _dutWithCommPkgScope.RemoveCommPkg(_commPkg1);

            // Assert
            Assert.AreEqual(1, _dutWithCommPkgScope.CommPkgs.Count);
            Assert.IsFalse(_dutWithCommPkgScope.CommPkgs.Contains(_commPkg1));
        }

        [TestMethod]
        public void AddParticipant_ShouldAddParticipantToParticipantList()
        {
            var participant = new Mock<Participant>();
            participant.SetupGet(p => p.Plant).Returns(TestPlant);

            _dutWithMcPkgScope.AddParticipant(participant.Object);

            Assert.AreEqual(5, _dutWithMcPkgScope.Participants.Count);
            Assert.IsTrue(_dutWithMcPkgScope.Participants.Contains(participant.Object));
        }

        [TestMethod]
        public void UpdateParticipant_ShouldUpdateParticipantInParticipantList()
        {
            Assert.IsTrue(_dutWithMcPkgScope.Participants.Contains(_externalParticipant));

            _dutWithMcPkgScope.UpdateParticipant(
                _externalParticipantId,
                Organization.Operation,
                IpoParticipantType.Person,
                null,
                "Kari",
                "Traa",
                "kari@test.com",
                new Guid("11111111-2222-2222-2222-333333333333"),
                2,
                "AAAAAAAAABA=");

            Assert.AreEqual(4, _dutWithMcPkgScope.Participants.Count);
            var updatedParticipant =
                _dutWithMcPkgScope.Participants.SingleOrDefault(p => p.Id == _externalParticipantId);
            Assert.IsNotNull(updatedParticipant);
            Assert.AreEqual(updatedParticipant.FirstName, "Kari");
            Assert.AreEqual(updatedParticipant.LastName, "Traa");
            Assert.AreEqual(updatedParticipant.AzureOid, new Guid("11111111-2222-2222-2222-333333333333"));
        }

        [TestMethod]
        public void RemoveParticipant_ShouldRemoveParticipantFromParticipantList()
        {
            // Arrange
            Assert.AreEqual(4, _dutWithMcPkgScope.Participants.Count);

            // Act
            _dutWithMcPkgScope.RemoveParticipant(_externalParticipant);

            // Assert
            Assert.AreEqual(3, _dutWithMcPkgScope.Participants.Count);
            Assert.IsFalse(_dutWithMcPkgScope.Participants.Contains(_externalParticipant));
        }

        [TestMethod]
        public void AddComment_ShouldAddCommentToCommentList()
        {
            Assert.AreEqual(1, _dutWithCommPkgScope.Comments.Count);

            var comment = new Comment(TestPlant, "New comment");
            _dutWithCommPkgScope.AddComment(comment);

            Assert.AreEqual(2, _dutWithCommPkgScope.Comments.Count);
            Assert.IsTrue(_dutWithCommPkgScope.Comments.Contains(comment));
        }

        [TestMethod]
        public void AddComment_ShouldAddAddCommentEvent()
        {
            var comment = new Comment(TestPlant, "New comment");
            _dutWithCommPkgScope.AddComment(comment);

            Assert.IsInstanceOfType(_dutWithCommPkgScope.PreSaveDomainEvents.Last(), typeof(CommentAddedEvent));
        }

        [TestMethod]
        public void RemoveComment_ShouldRemoveCommentFromCommentList()
        {
            Assert.AreEqual(1, _dutWithCommPkgScope.Comments.Count);

            _dutWithCommPkgScope.RemoveComment(_comment);

            Assert.AreEqual(0, _dutWithCommPkgScope.Comments.Count);
            Assert.IsFalse(_dutWithCommPkgScope.Comments.Contains(_comment));
        }

        [TestMethod]
        public void RemoveComment_ShouldAddRemoveCommentEvent()
        {
            _dutWithCommPkgScope.RemoveComment(_comment);

            Assert.IsInstanceOfType(_dutWithCommPkgScope.PreSaveDomainEvents.Last(), typeof(CommentRemovedEvent));
        }

        [TestMethod]
        public void RemoveAttachment_ShouldRemoveAttachment()
        {
            Assert.AreEqual(1, _dutWithMcPkgScope.Attachments.Count);
            Assert.AreEqual(_attachment, _dutWithMcPkgScope.Attachments.First());

            _dutWithMcPkgScope.RemoveAttachment(_attachment);

            Assert.AreEqual(0, _dutWithMcPkgScope.Attachments.Count);
        }

        [TestMethod]
        public void RemoveAttachment_ShouldAddRemoveAttachmentEvent()
        {
            _dutWithMcPkgScope.RemoveAttachment(_attachment);

            Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.Last(), typeof(AttachmentRemovedEvent));
        }

        [TestMethod]
        public void AddAttachment_ShouldAddAttachment()
        {
            var attachment = new Attachment(TestPlant, "A.txt");
            _dutWithMcPkgScope.AddAttachment(attachment);

            Assert.AreEqual(attachment, _dutWithMcPkgScope.Attachments.Last());
        }

        [TestMethod]
        public void AddAttachment_ShouldAddAddAttachmentEvent()
        {
            var attachment = new Attachment(TestPlant, "A.txt");
            _dutWithMcPkgScope.AddAttachment(attachment);

            Assert.AreEqual(attachment, _dutWithMcPkgScope.Attachments.Last());

            Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.Last(), typeof(AttachmentUploadedEvent));
        }

        [TestMethod]
        public void CompleteIpo_ShouldNotCompleteIpo_WhenIpoIsNotPlanned()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCommPkgScope.CompleteIpo(
                    _personParticipant,
                    ParticipantRowVersion,
                    _currentPerson,
                    new DateTime()));

        [TestMethod]
        public void CompleteIpo_ShouldCompleteIpo()
        {
            Assert.AreEqual(IpoStatus.Planned, _dutWithMcPkgScope.Status);

            _dutWithMcPkgScope.CompleteIpo(
                _personParticipant,
                ParticipantRowVersion,
                _currentPerson,
                new DateTime());

            Assert.AreEqual(IpoStatus.Completed, _dutWithMcPkgScope.Status);
            Assert.IsNotNull(_dutWithMcPkgScope.Participants.First().SignedAtUtc);
            Assert.AreEqual(_currentUserId, _dutWithMcPkgScope.Participants.First().SignedBy);
            Assert.IsNotNull(_dutWithMcPkgScope.CompletedAtUtc);
            Assert.AreEqual(_currentUserId, _dutWithMcPkgScope.CompletedBy);
        }

        [TestMethod]
        public void CompleteIpo_ShouldAddCompleteIpoEvent()
        {
            _dutWithMcPkgScope.CompleteIpo(
                _personParticipant,
                ParticipantRowVersion,
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.Last(), typeof(IpoCompletedEvent));
        }

        [TestMethod]
        public void AcceptIpo_ShouldNotAcceptIpo_WhenIpoIsNotCompleted()
            => Assert.ThrowsException<Exception>(()
                => _dutWithMcPkgScope.AcceptIpo(
                    _functionalRoleParticipant,
                    _functionalRoleParticipant.RowVersion.ConvertToString(),
                    _currentPerson,
                    new DateTime()));

        [TestMethod]
        public void AcceptIpo_ShouldAcceptIpo()
        {
            Assert.AreEqual(IpoStatus.Completed, _dutWithCommPkgScope.Status);

            _dutWithCommPkgScope.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            Assert.AreEqual(IpoStatus.Accepted, _dutWithCommPkgScope.Status);
            Assert.AreEqual(_currentUserId, _dutWithCommPkgScope.Participants.Single(p => p.SortKey == 1).SignedBy);
            Assert.IsNotNull(_dutWithCommPkgScope.Participants.Single(p => p.SortKey == 1).SignedAtUtc);
            Assert.AreEqual(_currentUserId, _dutWithCommPkgScope.AcceptedBy);
            Assert.IsNotNull(_dutWithCommPkgScope.AcceptedAtUtc);

        }

        [TestMethod]
        public void AcceptIpo_ShouldAddAcceptIpoEvent()
        {
            _dutWithCommPkgScope.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutWithCommPkgScope.PreSaveDomainEvents.Last(), typeof(IpoAcceptedEvent));
        }

        [TestMethod]
        public void SignIpo_ShouldNotSignIpo_WhenIpoIsCanceled()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCanceledStatus.SignIpo(
                    _functionalRoleParticipant,
                    _currentPerson,
                    _functionalRoleParticipant.RowVersion.ConvertToString()));

        [TestMethod]
        public void SignIpo_ShouldSignIpo()
        {
            _dutWithMcPkgScope.SignIpo(
                _personParticipant2,
                _currentPerson,
                _personParticipant2.RowVersion.ConvertToString());

            Assert.AreEqual(_currentUserId,
                _dutWithMcPkgScope.Participants.Single(p => p.AzureOid == _personParticipant2.AzureOid).SignedBy);
            Assert.IsNotNull(_dutWithMcPkgScope.Participants.Single(p => p.AzureOid == _personParticipant2.AzureOid).SignedAtUtc);
        }

        [TestMethod]
        public void SignIpo_ShouldAddSignIpoEvent()
        {
            _dutWithMcPkgScope.SignIpo(
                _personParticipant2,
                _currentPerson,
                _personParticipant2.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.Last(), typeof(IpoSignedEvent));
        }

        [TestMethod]
        public void EditIpo_ShouldNotEditIpo_WhenIpoIsNotPlanned()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            Assert.ThrowsException<Exception>(() =>
                _dutWithCommPkgScope.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    newStartTime,
                    newEndTime,
                    "outside"));
        }

        [TestMethod]
        public void EditIpo_ShouldNotEditIpo_WhenStartDateIsBeforeEndDate()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc);
            Assert.ThrowsException<Exception>(() =>
                _dutWithCommPkgScope.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    newStartTime,
                    newEndTime,
                    "outside"));
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo()
        {
            Assert.AreEqual(Title, _dutWithMcPkgScope.Title);
            Assert.AreEqual(Description, _dutWithMcPkgScope.Description);
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            _dutWithMcPkgScope.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP, 
                newStartTime,
                newEndTime,
                "outside");

            Assert.AreEqual("New Title", _dutWithMcPkgScope.Title);
            Assert.AreEqual("New description", _dutWithMcPkgScope.Description);
            Assert.AreEqual("outside", _dutWithMcPkgScope.Location);
            Assert.AreEqual(newStartTime, _dutWithMcPkgScope.StartTimeUtc);
            Assert.AreEqual(newEndTime, _dutWithMcPkgScope.EndTimeUtc);
        }

        [TestMethod]
        public void EditIpo_ShouldAddEditIpoEvent()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            _dutWithMcPkgScope.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                newStartTime,
                newEndTime,
                "outside");

            Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.Last(), typeof(IpoEditedEvent));
        }

        [TestMethod]
        public void Constructor_ShouldAddIpoCreatedEvent() 
            => Assert.IsInstanceOfType(_dutWithMcPkgScope.PreSaveDomainEvents.First(), typeof(IpoCreatedEvent));

        [TestMethod]
        public void CancelIpo_SetsStatusToCanceled()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));

            var dut = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            dut.SetCreated(_currentPerson);
            dut.CancelIpo(_currentPerson);
            Assert.AreEqual(dut.Status, IpoStatus.Canceled);
        }

        [TestMethod]
        public void CancelIpo_IpoIsAlreadyCanceled_ThrowsException()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
  
            var dut = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            dut.SetCreated(_currentPerson);
            dut.CancelIpo(_currentPerson);
            Assert.ThrowsException<Exception>(() => dut.CancelIpo(_currentPerson));
        }

        [TestMethod]
        public void CancelIpo_IpoIsAccepted_ThrowsException()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
            var creator = new Person(new Guid("12345678-1234-1234-1234-123456789123"), "Test", "Person", "tp", "tp@pcs.pcs");

            var dut = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            dut.SetCreated(creator);

            dut.CompleteIpo(_personParticipant, ParticipantRowVersion, creator, new DateTime());
            dut.AcceptIpo(_personParticipant, ParticipantRowVersion, creator, new DateTime());

            Assert.ThrowsException<Exception>(() => dut.CancelIpo(creator));
        }

        [TestMethod]
        public void CancelIpo_CallerIsNotCreator_ThrowsException()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
            var creator = new Person(new Guid("12345678-1234-1234-1234-123456789123"), "Test", "Person", "tp", "tp@pcs.pcs");
            var caller = new Person(new Guid("99999999-9999-9999-9999-999999999999"), "Another", "Person", "ap", "ap@pcs.pcs");

            // Set caller ID to a different ID than the creator
            caller
                .GetType()
                .GetProperty(nameof(Person.Id))
                ?.SetValue(caller, 1, null);

            var dut = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            dut.SetCreated(creator);

            Assert.ThrowsException<InvalidOperationException>(() => dut.CancelIpo(caller));
        }

        [TestMethod]
        public void CancelIpo_CallerIsNull_ThrowsException()
        {
            var dut = new Invitation(
                TestPlant,
                ProjectName,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null);

            Assert.ThrowsException<ArgumentNullException>(() => dut.CancelIpo(null));
        }
    }
}
