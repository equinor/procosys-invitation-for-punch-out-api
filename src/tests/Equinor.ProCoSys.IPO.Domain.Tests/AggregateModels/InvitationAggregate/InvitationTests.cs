using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
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
        private Invitation _dutDpIpo;
        private Invitation _dutMdpIpo;
        private Invitation _dutWithCompletedStatus;
        private Invitation _dutWithAcceptedStatus;
        private Invitation _dutWithCanceledStatus;
        private Invitation _dutWithSignedParticipant;
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
        private const int ProjectId = 132;
        private static readonly Project project1 = new Project(TestPlant, $"{ProjectName} project", $"Description of {ProjectName} project");
        private const string Title = "Title A";
        private const string Title2 = "Title B";
        private const string Description = "Description A";
        private const string System = "1|2";
        private const string ParticipantRowVersion = "AAAAAAAAABA=";

        [TestInitialize]
        public void Setup()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));
            _mcPkg1 = new McPkg(TestPlant, project1, "Comm1", "Mc1", "MC D", System);
            _mcPkg2 = new McPkg(TestPlant, project1, "Comm1", "Mc2", "MC D 2", System);
            _commPkg1 = new CommPkg(TestPlant, project1, "Comm1", "Comm D", "OK", "1|2");
            _commPkg2 = new CommPkg(TestPlant, project1, "Comm2", "Comm D 2", "OK", "1|2");

            _dutDpIpo = new Invitation(
                TestPlant,
                project1,
                Title,
                Description,
                DisciplineType.DP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                new List<McPkg>{ _mcPkg1, _mcPkg2 },
                null);

            _dutMdpIpo = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> { _commPkg1, _commPkg2 });

            _dutWithCompletedStatus = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> {_commPkg1, _commPkg2});

            _dutWithAcceptedStatus = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> {_commPkg1, _commPkg2});

            _dutWithCanceledStatus = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> { _commPkg1, _commPkg2 });

            _dutWithSignedParticipant = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> { _commPkg1, _commPkg2 });

            _personParticipantId = 10033;
            _functionalRoleParticipantId = 3;
            _externalParticipantId = 967;

            _comment = new Comment(TestPlant, "Comment text");
            _dutWithCompletedStatus.AddComment(_comment);
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
                3);

            _attachment = new Attachment(TestPlant, "filename.txt");
            _currentPerson = new Person(new Guid(), null, null, null, null);
            _currentPerson.SetProtectedIdForTesting(_currentUserId);

            _dutDpIpo.AddParticipant(_personParticipant);
            _dutDpIpo.AddParticipant(_functionalRoleParticipant);
            _dutDpIpo.AddParticipant(_externalParticipant);
            _dutDpIpo.AddParticipant(_personParticipant2);
            _dutDpIpo.AddAttachment(_attachment);
            _dutWithCompletedStatus.AddParticipant(_personParticipant);
            _dutWithCompletedStatus.AddParticipant(_functionalRoleParticipant);
            _dutWithCompletedStatus.CompleteIpo(
                _personParticipant,
                _personParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());
            _dutWithAcceptedStatus.AddParticipant(_personParticipant);
            _dutWithAcceptedStatus.AddParticipant(_functionalRoleParticipant);
            _dutWithAcceptedStatus.CompleteIpo(_personParticipant, _personParticipant.RowVersion.ConvertToString(),
                _currentPerson, new DateTime());
            _dutWithAcceptedStatus.AcceptIpo(_functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(), _currentPerson, new DateTime());
            _dutWithCanceledStatus.SetCreated(_currentPerson);
            _dutWithCanceledStatus.CancelIpo(_currentPerson);
            _dutWithSignedParticipant.AddParticipant(_personParticipant2);
            _dutWithSignedParticipant.SignIpo(
                _personParticipant2,
                _currentPerson,
                _personParticipant2.RowVersion.ConvertToString());
        }

        #region Constructor
        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dutDpIpo.Plant);
            Assert.AreEqual(ProjectId, _dutDpIpo.ProjectId);
            Assert.AreEqual(Title, _dutDpIpo.Title);
            Assert.AreEqual(Description, _dutDpIpo.Description);
            Assert.AreEqual(DisciplineType.DP, _dutDpIpo.Type);
            Assert.AreEqual(4, _dutDpIpo.Participants.Count);
            Assert.AreEqual(2, _dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void Constructor_ShouldAddUniqueMcPkgs()
        {
            var dutDpIpo = new Invitation(
                TestPlant,
                project1,
                Title,
                Description,
                DisciplineType.DP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                new List<McPkg>{ _mcPkg1, _mcPkg2, _mcPkg1, _mcPkg2 },
                null);
            Assert.AreEqual(2, dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void Constructor_ShouldAddUniqueCommPkgs()
        {
            var dutMdpIpo = new Invitation(
                TestPlant,
                project1,
                Title2,
                Description,
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                null,
                null,
                new List<CommPkg> { _commPkg1, _commPkg2, _commPkg1, _commPkg2 });
            Assert.AreEqual(2, dutMdpIpo.CommPkgs.Count);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenTitleNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    null,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> {_commPkg1})
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
                    null,
                    null,
                    new List<CommPkg> { _commPkg1 })
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenScopeNotGiven() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    Title,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    null)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenMcPkgScopeOnMDp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    Title,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> {_mcPkg1},
                    null)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenBothScopeOnMDp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    Title,
                    Description,
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> {_mcPkg1},
                    new List<CommPkg> {_commPkg1})
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenCommPkgScopeOnDp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    Title,
                    Description,
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    null,
                    new List<CommPkg> {_commPkg1})
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenBothScopeOnDp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                new Invitation(
                    TestPlant,
                    project1,
                    Title,
                    Description,
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> {_mcPkg1},
                    new List<CommPkg> {_commPkg1})
            );

        [TestMethod]
        public void Constructor_ShouldAddIpoCreatedEvent()
            => Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.First(), typeof(IpoCreatedEvent));
        #endregion

        #region Participant
        [TestMethod]
        public void AddParticipant_ShouldThrowException_WhenParticipantNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.AddParticipant(null));

        [TestMethod]
        public void RemoveParticipant_ShouldThrowException_WhenParticipantNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.RemoveParticipant(null));



        [TestMethod]
        public void AddParticipant_ShouldAddParticipantToParticipantList()
        {
            var participant = new Mock<Participant>();
            participant.SetupGet(p => p.Plant).Returns(TestPlant);

            _dutDpIpo.AddParticipant(participant.Object);

            Assert.AreEqual(5, _dutDpIpo.Participants.Count);
            Assert.IsTrue(_dutDpIpo.Participants.Contains(participant.Object));
        }

        [TestMethod]
        public void UpdateParticipant_ShouldUpdateParticipantInParticipantList()
        {
            Assert.IsTrue(_dutDpIpo.Participants.Contains(_externalParticipant));

            _dutDpIpo.UpdateParticipant(
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

            Assert.AreEqual(4, _dutDpIpo.Participants.Count);
            var updatedParticipant =
                _dutDpIpo.Participants.SingleOrDefault(p => p.Id == _externalParticipantId);
            Assert.IsNotNull(updatedParticipant);
            Assert.AreEqual(updatedParticipant.FirstName, "Kari");
            Assert.AreEqual(updatedParticipant.LastName, "Traa");
            Assert.AreEqual(updatedParticipant.AzureOid, new Guid("11111111-2222-2222-2222-333333333333"));
        }

        [TestMethod]
        public void RemoveParticipant_ShouldRemoveParticipantFromParticipantList()
        {
            // Arrange
            Assert.AreEqual(4, _dutDpIpo.Participants.Count);

            // Act
            _dutDpIpo.RemoveParticipant(_externalParticipant);

            // Assert
            Assert.AreEqual(3, _dutDpIpo.Participants.Count);
            Assert.IsFalse(_dutDpIpo.Participants.Contains(_externalParticipant));
        }
        #endregion

        #region Attachment
        [TestMethod]
        public void AddAttachment_ShouldThrowException_WhenAttachmentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.AddAttachment(null));

        [TestMethod]
        public void RemoveAttachment_ShouldThrowException_WhenAttachmentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.RemoveAttachment(null));

        [TestMethod]
        public void RemoveAttachment_ShouldRemoveAttachment()
        {
            Assert.AreEqual(1, _dutDpIpo.Attachments.Count);
            Assert.AreEqual(_attachment, _dutDpIpo.Attachments.First());

            _dutDpIpo.RemoveAttachment(_attachment);

            Assert.AreEqual(0, _dutDpIpo.Attachments.Count);
        }

        [TestMethod]
        public void RemoveAttachment_ShouldAddRemoveAttachmentEvent()
        {
            _dutDpIpo.RemoveAttachment(_attachment);

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(AttachmentRemovedEvent));
        }

        [TestMethod]
        public void AddAttachment_ShouldAddAttachment()
        {
            var attachment = new Attachment(TestPlant, "A.txt");
            _dutDpIpo.AddAttachment(attachment);

            Assert.AreEqual(attachment, _dutDpIpo.Attachments.Last());
        }

        [TestMethod]
        public void AddAttachment_ShouldAddAddAttachmentEvent()
        {
            var attachment = new Attachment(TestPlant, "A.txt");
            _dutDpIpo.AddAttachment(attachment);

            Assert.AreEqual(attachment, _dutDpIpo.Attachments.Last());

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(AttachmentUploadedEvent));
        }
        #endregion

        #region Comment
        [TestMethod]
        public void AddComment_ShouldThrowException_WhenCommentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.AddComment(null));

        [TestMethod]
        public void RemoveComment_ShouldThrowException_WhenCommentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutDpIpo.RemoveComment(null));

        [TestMethod]
        public void AddComment_ShouldAddCommentToCommentList()
        {
            Assert.AreEqual(1, _dutWithCompletedStatus.Comments.Count);

            var comment = new Comment(TestPlant, "New comment");
            _dutWithCompletedStatus.AddComment(comment);

            Assert.AreEqual(2, _dutWithCompletedStatus.Comments.Count);
            Assert.IsTrue(_dutWithCompletedStatus.Comments.Contains(comment));
        }

        [TestMethod]
        public void AddComment_ShouldAddAddCommentEvent()
        {
            var comment = new Comment(TestPlant, "New comment");
            _dutWithCompletedStatus.AddComment(comment);

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PreSaveDomainEvents.Last(), typeof(CommentAddedEvent));
        }

        [TestMethod]
        public void RemoveComment_ShouldRemoveCommentFromCommentList()
        {
            Assert.AreEqual(1, _dutWithCompletedStatus.Comments.Count);

            _dutWithCompletedStatus.RemoveComment(_comment);

            Assert.AreEqual(0, _dutWithCompletedStatus.Comments.Count);
            Assert.IsFalse(_dutWithCompletedStatus.Comments.Contains(_comment));
        }

        [TestMethod]
        public void RemoveComment_ShouldAddRemoveCommentEvent()
        {
            _dutWithCompletedStatus.RemoveComment(_comment);

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PreSaveDomainEvents.Last(), typeof(CommentRemovedEvent));
        }
        #endregion

        #region Edit
        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenIpoIsNotPlanned()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            Assert.ThrowsException<Exception>(() =>
                _dutWithCompletedStatus.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    newStartTime,
                    newEndTime,
                    "outside",
                    null,
                    null));
        }

        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenStartDateIsBeforeEndDate()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 11, 0, 0, DateTimeKind.Utc);
            Assert.ThrowsException<Exception>(() =>
                _dutWithCompletedStatus.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    newStartTime,
                    newEndTime,
                    "outside",
                    null,
                    null));
        }

        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenAddingCommPkgToDp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dutDpIpo.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    "outside",
                    _dutDpIpo.McPkgs.ToList(),
                    new List<CommPkg> {_commPkg1}));

        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenAddingMcPkgToMdp() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dutMdpIpo.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    "outside",
                    new List<McPkg> {_mcPkg1},
                    _dutMdpIpo.CommPkgs.ToList()));

        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenSettingDpOnIpoWithCommScope() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dutMdpIpo.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    "outside",
                    null,
                    _dutMdpIpo.CommPkgs.ToList()));

        [TestMethod]
        public void EditIpo_ShouldThrowException_WhenSettingMdpOnIpoWithMcScope() =>
            Assert.ThrowsException<ArgumentException>(() =>
                _dutDpIpo.EditIpo(
                    "New Title",
                    "New description",
                    DisciplineType.MDP,
                    new DateTime(),
                    new DateTime(),
                    "outside",
                    _dutDpIpo.McPkgs.ToList(),
                    null));

        [TestMethod]
        public void EditIpo_ShouldEditIpo_AddMcPkgScope()
        {
            var newMcPkg = new McPkg(TestPlant, project1, "Comm2", "Mc3", "MC D", System);

            Assert.AreEqual(2, _dutDpIpo.McPkgs.Count);

            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                "outside",
                new List<McPkg> { _mcPkg1, _mcPkg2, newMcPkg },
                null);

            Assert.AreEqual(3, _dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_AddUniqueMcPkgs()
        {
            var newMcPkg = new McPkg(TestPlant, project1, "Comm2", "Mc3", "MC D", System);

            Assert.AreEqual(2, _dutDpIpo.McPkgs.Count);

            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                "outside",
                new List<McPkg> { _mcPkg1, _mcPkg2, newMcPkg, _mcPkg1, _mcPkg2 },
                null);

            Assert.AreEqual(3, _dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_AddCommPkgScope()
        {
            var newCommPkg = new CommPkg(TestPlant, project1, "Comm3", "D", "OK", System);

            Assert.AreEqual(2, _dutMdpIpo.CommPkgs.Count);

            _dutMdpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                "outside",
                null,
                new List<CommPkg> {_commPkg1, _commPkg2, newCommPkg});

            Assert.AreEqual(3, _dutMdpIpo.CommPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_AddUniqueCommPkgs()
        {
            var newCommPkg = new CommPkg(TestPlant, project1, "Comm3", "D", "OK", System);

            Assert.AreEqual(2, _dutMdpIpo.CommPkgs.Count);

            _dutMdpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                "outside",
                null,
                new List<CommPkg> {_commPkg1, _commPkg2, newCommPkg, _commPkg1, _commPkg2});

            Assert.AreEqual(3, _dutMdpIpo.CommPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_RemoveMcPkg()
        {
            Assert.AreEqual(2, _dutDpIpo.McPkgs.Count);

            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                "outside",
                new List<McPkg> { _mcPkg1 },
                null);

            Assert.AreEqual(1, _dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_RemoveCommPkg()
        {
            Assert.AreEqual(2, _dutMdpIpo.CommPkgs.Count);

            _dutMdpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                "outside",
                null,
                new List<CommPkg> { _commPkg1 });

            Assert.AreEqual(1, _dutMdpIpo.CommPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_ChangeDpToMdp()
        {
            Assert.AreEqual(2, _dutDpIpo.McPkgs.Count);

            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.MDP,
                new DateTime(),
                new DateTime(),
                "outside",
                null,
                new List<CommPkg> { _commPkg1 });

            Assert.AreEqual(1, _dutDpIpo.CommPkgs.Count);
            Assert.AreEqual(0, _dutDpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo_ChangeMdpToDp()
        {
            Assert.AreEqual(2, _dutMdpIpo.CommPkgs.Count);

            _dutMdpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                new DateTime(),
                new DateTime(),
                "outside",
                new List<McPkg> {_mcPkg1},
                null);

            Assert.AreEqual(0, _dutMdpIpo.CommPkgs.Count);
            Assert.AreEqual(1, _dutMdpIpo.McPkgs.Count);
        }

        [TestMethod]
        public void EditIpo_ShouldEditIpo()
        {
            Assert.AreEqual(Title, _dutDpIpo.Title);
            Assert.AreEqual(Description, _dutDpIpo.Description);
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                newStartTime,
                newEndTime,
                "outside",
                new List<McPkg> { _mcPkg1, _mcPkg2 },
                null);

            Assert.AreEqual("New Title", _dutDpIpo.Title);
            Assert.AreEqual("New description", _dutDpIpo.Description);
            Assert.AreEqual("outside", _dutDpIpo.Location);
            Assert.AreEqual(newStartTime, _dutDpIpo.StartTimeUtc);
            Assert.AreEqual(newEndTime, _dutDpIpo.EndTimeUtc);
        }

        [TestMethod]
        public void EditIpo_ShouldAddEditIpoEvent()
        {
            var newStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
            var newEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
            _dutDpIpo.EditIpo(
                "New Title",
                "New description",
                DisciplineType.DP,
                newStartTime,
                newEndTime,
                "outside",
                new List<McPkg> { _mcPkg1, _mcPkg2 },
                null);

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(IpoEditedEvent));
        }
        #endregion

        #region UpdateAttendedStatus
        [TestMethod]
        public void UpdateAttendedStatus_ShouldNotUpdateAttendedStatus_WhenIpoIsCanceled()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCanceledStatus.UpdateAttendedStatus(
                    _functionalRoleParticipant,
                    true,
                    _functionalRoleParticipant.RowVersion.ConvertToString()));

        [TestMethod]
        public void UpdateAttendedStatus_ShouldAddUpdateAttendedStatusEvent()
        {
            _dutDpIpo.UpdateAttendedStatus(
                _personParticipant,
                true,
                _personParticipant.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(AttendedStatusUpdatedEvent));
        }

        [TestMethod]
        public void UpdateAttendedStatus_ShouldUpdateAttendedStatus()
        {
            Assert.IsFalse(_dutDpIpo.Participants.First().Attended);
            Assert.IsFalse(_dutDpIpo.Participants.First().IsAttendedTouched);

            _dutDpIpo.UpdateAttendedStatus(
                _personParticipant,
                true,
                _personParticipant.RowVersion.ConvertToString());

            Assert.IsTrue(_dutDpIpo.Participants.First().Attended);
            Assert.IsTrue(_dutDpIpo.Participants.First().IsAttendedTouched);
        }
        #endregion


        #region UpdateNote
        [TestMethod]
        public void UpdateNote_ShouldNotUpdateNote_WhenIpoIsCanceled()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCanceledStatus.UpdateNote(
                    _functionalRoleParticipant,
                    "note",
                    _functionalRoleParticipant.RowVersion.ConvertToString()));

        [TestMethod]
        public void UpdateNote_ShouldAddUpdateNoteEvent()
        {
            _dutDpIpo.UpdateNote(
                _personParticipant,
                "note",
                _personParticipant.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(NoteUpdatedEvent));
        }

        [TestMethod]
        public void UpdateNote_ShouldUpdateNote()
        {
            _dutDpIpo.UpdateNote(
                _personParticipant,
                "note",
                _personParticipant.RowVersion.ConvertToString());

            Assert.AreEqual("note", _dutDpIpo.Participants.First().Note);
        }
        #endregion

        #region Complete
        [TestMethod]
        public void CompleteIpo_ShouldNotCompleteIpo_WhenIpoIsNotPlanned()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCompletedStatus.CompleteIpo(
                    _personParticipant,
                    ParticipantRowVersion,
                    _currentPerson,
                    new DateTime()));

        [TestMethod]
        public void CompleteIpo_ShouldCompleteIpo()
        {
            Assert.AreEqual(IpoStatus.Planned, _dutDpIpo.Status);

            _dutDpIpo.CompleteIpo(
                _personParticipant,
                ParticipantRowVersion,
                _currentPerson,
                new DateTime());

            Assert.AreEqual(IpoStatus.Completed, _dutDpIpo.Status);
            Assert.IsNotNull(_dutDpIpo.Participants.First().SignedAtUtc);
            Assert.AreEqual(_currentUserId, _dutDpIpo.Participants.First().SignedBy);
            Assert.IsNotNull(_dutDpIpo.CompletedAtUtc);
            Assert.AreEqual(_currentUserId, _dutDpIpo.CompletedBy);
        }

        [TestMethod]
        public void CompleteIpo_ShouldAddCompleteIpoPreSaveEvent()
        {
            _dutDpIpo.CompleteIpo(
                _personParticipant,
                ParticipantRowVersion,
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(IpoCompletedEvent));
        }

        [TestMethod]
        public void CompleteIpo_ShouldAddCompleteIpoPostSaveEvent()
        {
            _dutDpIpo.CompleteIpo(
                _personParticipant,
                ParticipantRowVersion,
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutDpIpo.PostSaveDomainEvents.Last(), typeof(Events.PostSave.IpoCompletedEvent));
        }
        #endregion

        #region Uncomplete
        [TestMethod]
        public void UnCompleteIpo_ShouldNotUnCompleteIpo_WhenIpoIsNotCompleted()
            => Assert.ThrowsException<Exception>(()
                => _dutDpIpo.UnCompleteIpo(
                    _personParticipant,
                    ParticipantRowVersion));

        [TestMethod]
        public void UnCompleteIpo_ShouldUnCompleteIpo()
        {
            Assert.AreEqual(IpoStatus.Completed, _dutWithCompletedStatus.Status);

            _dutWithCompletedStatus.UnCompleteIpo(
                _personParticipant,
                ParticipantRowVersion);

            Assert.AreEqual(IpoStatus.Planned, _dutWithCompletedStatus.Status);
            Assert.IsNull(_dutWithCompletedStatus.Participants.First().SignedAtUtc);
            Assert.IsNull(_dutWithCompletedStatus.CompletedBy);
            Assert.IsNull(_dutWithCompletedStatus.CompletedAtUtc);
        }

        [TestMethod]
        public void UnCompleteIpo_ShouldAddUnCompleteIpoPreSaveEvent()
        {
            _dutWithCompletedStatus.UnCompleteIpo(
                _personParticipant,
                ParticipantRowVersion);

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PreSaveDomainEvents.Last(), typeof(IpoUnCompletedEvent));
        }

        [TestMethod]
        public void UnCompleteIpo_ShouldAddUnCompleteIpoPostSaveEvent()
        {
            _dutWithCompletedStatus.UnCompleteIpo(
                _personParticipant,
                ParticipantRowVersion);

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PostSaveDomainEvents.Last(), typeof(Events.PostSave.IpoUnCompletedEvent));
        }
        #endregion

        #region Accept
        [TestMethod]
        public void AcceptIpo_ShouldNotAcceptIpo_WhenIpoIsNotCompleted()
            => Assert.ThrowsException<Exception>(()
                => _dutDpIpo.AcceptIpo(
                    _functionalRoleParticipant,
                    _functionalRoleParticipant.RowVersion.ConvertToString(),
                    _currentPerson,
                    new DateTime()));

        [TestMethod]
        public void AcceptIpo_ShouldAcceptIpo()
        {
            Assert.AreEqual(IpoStatus.Completed, _dutWithCompletedStatus.Status);

            _dutWithCompletedStatus.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            Assert.AreEqual(IpoStatus.Accepted, _dutWithCompletedStatus.Status);
            var participant = _dutWithCompletedStatus.Participants.Single(p => p.SortKey == 1);
            Assert.AreEqual(_currentUserId, participant.SignedBy);
            Assert.IsNotNull(participant.SignedAtUtc);
            Assert.AreEqual(_currentUserId, _dutWithCompletedStatus.AcceptedBy);
            Assert.IsNotNull(_dutWithCompletedStatus.AcceptedAtUtc);

        }

        [TestMethod]
        public void AcceptIpo_ShouldAddAcceptIpoPreSaveEvent()
        {
            _dutWithCompletedStatus.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PreSaveDomainEvents.Last(), typeof(IpoAcceptedEvent));
        }

        [TestMethod]
        public void AcceptIpo_ShouldAddAcceptIpoPostSaveEvent()
        {
            _dutWithCompletedStatus.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PostSaveDomainEvents.Last(), typeof(Events.PostSave.IpoAcceptedEvent));
        }
        #endregion

        #region Unaccept
        [TestMethod]
        public void UnAcceptIpo_ShouldAddUnAcceptIpoPreSaveEvent()
        {
            _dutWithCompletedStatus.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            _dutWithCompletedStatus.UnAcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PreSaveDomainEvents.Last(), typeof(IpoUnAcceptedEvent));
        }

        [TestMethod]
        public void UnAcceptIpo_ShouldAddUnAcceptIpoPostSaveEvent()
        {
            _dutWithCompletedStatus.AcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString(),
                _currentPerson,
                new DateTime());

            _dutWithCompletedStatus.UnAcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutWithCompletedStatus.PostSaveDomainEvents.Last(), typeof(Events.PostSave.IpoUnAcceptedEvent));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UnAcceptUnAcceptedIpo_ShouldThrowAcception()
        {
            _dutWithCompletedStatus.UnAcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString());
        }

        [TestMethod]
        public void UnAcceptIpo_ShouldNotUnAcceptIpo_WhenIpoIsNotAccepted()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCompletedStatus.UnAcceptIpo(
                    _functionalRoleParticipant,
                    _functionalRoleParticipant.RowVersion.ConvertToString()));

        [TestMethod]
        public void UnAcceptIpo_ShouldUnAcceptIpo()
        {
            Assert.AreEqual(IpoStatus.Accepted, _dutWithAcceptedStatus.Status);

            _dutWithAcceptedStatus.UnAcceptIpo(
                _functionalRoleParticipant,
                _functionalRoleParticipant.RowVersion.ConvertToString());

            Assert.AreEqual(IpoStatus.Completed, _dutWithAcceptedStatus.Status);
            var constructionCompany = _dutWithAcceptedStatus.Participants.First(p => p.Organization == Organization.ConstructionCompany);
            Assert.IsNull(constructionCompany.SignedAtUtc);
            Assert.IsNull(_dutWithAcceptedStatus.AcceptedBy);
            Assert.IsNull(_dutWithAcceptedStatus.AcceptedAtUtc);
        }
        #endregion

        #region Sign
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
            _dutDpIpo.SignIpo(
                _personParticipant2,
                _currentPerson,
                _personParticipant2.RowVersion.ConvertToString());

            var participant = _dutDpIpo.Participants.Single(p => p.AzureOid == _personParticipant2.AzureOid);
            Assert.AreEqual(_currentUserId, participant.SignedBy);
            Assert.IsNotNull(participant.SignedAtUtc);
        }

        [TestMethod]
        public void SignIpo_ShouldAddSignIpoEvent()
        {
            _dutDpIpo.SignIpo(
                _personParticipant2,
                _currentPerson,
                _personParticipant2.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutDpIpo.PreSaveDomainEvents.Last(), typeof(IpoSignedEvent));
        }
        #endregion

        #region Unsign
        [TestMethod]
        public void UnSignIpo_ShouldNotSignIpo_WhenIpoIsCanceled()
            => Assert.ThrowsException<Exception>(()
                => _dutWithCanceledStatus.UnSignIpo(
                    _functionalRoleParticipant,
                    _functionalRoleParticipant.RowVersion.ConvertToString()));

        [TestMethod]
        public void UnSignIpo_ShouldUnSignIpo()
        {
            var participant = _dutWithSignedParticipant.Participants.Single(p => p.AzureOid == _personParticipant2.AzureOid);
            Assert.AreEqual(_currentUserId, participant.SignedBy);

            _dutWithSignedParticipant.UnSignIpo(
                _personParticipant2,
                _personParticipant2.RowVersion.ConvertToString());

            Assert.IsNull(participant.SignedBy);
            Assert.IsNull(participant.SignedAtUtc);
        }

        [TestMethod]
        public void UnSignIpo_ShouldAddUnSignIpoEvent()
        {
            _dutWithSignedParticipant.UnSignIpo(
                _personParticipant2,
                _personParticipant2.RowVersion.ConvertToString());

            Assert.IsInstanceOfType(_dutWithSignedParticipant.PreSaveDomainEvents.Last(), typeof(IpoUnSignedEvent));
        }
        #endregion

        #region Cancel
        [TestMethod]
        public void CancelIpo_SetsStatusToCanceled()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));

            var dut = new Invitation(
                TestPlant,
                project1,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                null,
                new List<CommPkg>{_commPkg1});

            dut.SetCreated(_currentPerson);
            dut.CancelIpo(_currentPerson);
            Assert.AreEqual(dut.Status, IpoStatus.Canceled);
            Assert.AreEqual(1, dut.PostSaveDomainEvents.Count);
        }

        [TestMethod]
        public void CancelIpo_IpoIsAlreadyCanceled_ThrowsException()
        {
            TimeService.SetProvider(new ManualTimeProvider(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc)));

            var dut = new Invitation(
                TestPlant,
                project1,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                null,
                new List<CommPkg> { _commPkg1 });

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
                project1,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                null,
                new List<CommPkg> { _commPkg1 });

            dut.SetCreated(creator);

            dut.CompleteIpo(_personParticipant, ParticipantRowVersion, creator, new DateTime());
            dut.AcceptIpo(_personParticipant, ParticipantRowVersion, creator, new DateTime());

            Assert.ThrowsException<Exception>(() => dut.CancelIpo(creator));
        }

        [TestMethod]
        public void CancelIpo_CallerIsNull_ThrowsException()
        {
            var dut = new Invitation(
                TestPlant,
                project1,
                Title,
                Description,
                DisciplineType.MDP,
                new DateTime(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 8, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                null,
                new List<CommPkg> { _commPkg1 });

            Assert.ThrowsException<ArgumentNullException>(() => dut.CancelIpo(null));
        }
        #endregion
    }
}
