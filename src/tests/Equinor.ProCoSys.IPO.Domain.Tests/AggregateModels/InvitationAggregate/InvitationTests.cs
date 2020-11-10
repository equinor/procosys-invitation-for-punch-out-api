﻿using System;
using System.Linq;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
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
        private Participant _personParticipant;
        private Participant _functionalRoleParticipant;
        private Participant _externalParticipant;
        private int _personParticipantId;
        private int _functionalRoleParticipantId;
        private int _externalParticipantId;
        private McPkg _mcPkg1;
        private McPkg _mcPkg2;
        private CommPkg _commPkg1;
        private CommPkg _commPkg2;
        private const string TestPlant = "PlantA";
        private const string ProjectName = "ProjectName";
        private const string Title = "Title A";
        private const string Title2 = "Title B";
        private const string Description = "Description A";

        [TestInitialize]
        public void Setup()
        {
            _dutWithMcPkgScope = new Invitation(TestPlant, ProjectName, Title, Description, DisciplineType.MDP);
            _dutWithCommPkgScope = new Invitation(TestPlant, ProjectName, Title2, Description, DisciplineType.MDP);
            _personParticipantId = 10033;
            _functionalRoleParticipantId = 3;
            _externalParticipantId = 967;

            _mcPkg1 = new McPkg(TestPlant, ProjectName, "Comm1", "Mc1", "MC D");
            _mcPkg2 = new McPkg(TestPlant, ProjectName, "Comm1", "Mc2", "MC D 2");
            _commPkg1 = new CommPkg(TestPlant, ProjectName, "Comm1", "Comm D", "OK");
            _commPkg2 = new CommPkg(TestPlant, ProjectName, "Comm2", "Comm D 2", "OK");

            _personParticipant = new Participant(TestPlant, Organization.Contractor, IpoParticipantType.Person, null, "Ola", "Nordmann", "ola@test.com", new Guid("11111111-1111-2222-2222-333333333333"), 0);
            _personParticipant.SetProtectedIdForTesting(_personParticipantId);
            _functionalRoleParticipant = new Participant(TestPlant, Organization.ConstructionCompany, IpoParticipantType.FunctionalRole, "FR1", null, null, "fr1@test.com", null, 1);
            _functionalRoleParticipant.SetProtectedIdForTesting(_functionalRoleParticipantId);
            _externalParticipant = new Participant(TestPlant, Organization.External, IpoParticipantType.Person, null, null, null, "external@ext.com", null, 2);
            _externalParticipant.SetProtectedIdForTesting(_externalParticipantId);

            _dutWithMcPkgScope.AddParticipant(_personParticipant);
            _dutWithMcPkgScope.AddParticipant(_functionalRoleParticipant);
            _dutWithMcPkgScope.AddParticipant(_externalParticipant);
            _dutWithMcPkgScope.AddMcPkg(_mcPkg1);
            _dutWithMcPkgScope.AddMcPkg(_mcPkg2);
            _dutWithCommPkgScope.AddCommPkg(_commPkg1);
            _dutWithCommPkgScope.AddCommPkg(_commPkg2);
        }

        [TestMethod]
        public void Constructor_ShouldSetProperties()
        {
            Assert.AreEqual(TestPlant, _dutWithMcPkgScope.Plant);
            Assert.AreEqual(ProjectName, _dutWithMcPkgScope.ProjectName);
            Assert.AreEqual(Title, _dutWithMcPkgScope.Title);
            Assert.AreEqual(Description, _dutWithMcPkgScope.Description);
            Assert.AreEqual(DisciplineType.MDP, _dutWithMcPkgScope.Type);
            Assert.AreEqual(3, _dutWithMcPkgScope.Participants.Count);
            Assert.AreEqual(2, _dutWithMcPkgScope.McPkgs.Count);
        }

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenTitleNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Invitation(TestPlant, ProjectName, null, Description, DisciplineType.MDP)
            );

        [TestMethod]
        public void Constructor_ShouldThrowException_WhenProjectNameNotGiven() =>
            Assert.ThrowsException<ArgumentNullException>(() =>
                new Invitation(TestPlant, null, Title, Description, DisciplineType.MDP)
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
        public void RemoveMcPkg_ShouldThrowException_WhenMcPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveMcPkg(null));

        [TestMethod]
        public void RemoveCommPkg_ShouldThrowException_WhenCommPkgNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveCommPkg(null));

        [TestMethod]
        public void AddParticipant_ShouldAddParticipantToParticipantList()
        {
            var participant = new Mock<Participant>();
            participant.SetupGet(p => p.Plant).Returns(TestPlant);

            _dutWithMcPkgScope.AddParticipant(participant.Object);

            Assert.AreEqual(4, _dutWithMcPkgScope.Participants.Count);
            Assert.IsTrue(_dutWithMcPkgScope.Participants.Contains(participant.Object));
        }

        [TestMethod]
        public void UpdateParticipant_ShouldUpdateParticipantInParticipantList()
        {
            Assert.IsTrue(_dutWithMcPkgScope.Participants.Contains(_externalParticipant));

            _dutWithMcPkgScope.UpdateParticipant(_externalParticipantId, Organization.Operation, IpoParticipantType.Person, null, "Kari", "Traa", "kari@test.com", new Guid("11111111-2222-2222-2222-333333333333"), 2, "AAAAAAAAABA=");

            Assert.AreEqual(3, _dutWithMcPkgScope.Participants.Count);
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
            Assert.AreEqual(3, _dutWithMcPkgScope.Participants.Count);

            // Act
            _dutWithMcPkgScope.RemoveParticipant(_externalParticipant);

            // Assert
            Assert.AreEqual(2, _dutWithMcPkgScope.Participants.Count);
            Assert.IsFalse(_dutWithMcPkgScope.Participants.Contains(_externalParticipant));
        }

        [TestMethod]
        public void RemoveParticipant_ShouldThrowException_WhenParticipantNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveParticipant(null));

        [TestMethod]
        public void RemoveAttachment_ShouldThrowException_WhenAttachmentNotGiven()
            => Assert.ThrowsException<ArgumentNullException>(() => _dutWithMcPkgScope.RemoveAttachment(null));

        [TestMethod]
        public void AddAttachment_ShouldAddAttachment()
        {
            var attachment = new Attachment(TestPlant, "A.txt");
            _dutWithMcPkgScope.AddAttachment(attachment);

            Assert.AreEqual(attachment, _dutWithMcPkgScope.Attachments.First());
        }
    }
}