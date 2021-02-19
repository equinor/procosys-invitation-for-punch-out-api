using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Query.GetAttachments;
using Equinor.ProCoSys.IPO.Query.GetComments;
using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Equinor.ProCoSys.IPO.Query.GetHistory;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs;
using Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject;
using Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Authorizations
{
    [TestClass]
    public class AccessValidatorTests
    {
        private AccessValidator _dut;
        private Mock<IProjectAccessChecker> _projectAccessCheckerMock;
        private Mock<ILogger<AccessValidator>> _loggerMock;
        private Mock<ICurrentUserProvider> _currentUserProviderMock;
        private const int _invitationIdWithAccessToProject = 1;
        private const int _invitationIdWithoutAccessToProject = 2;
        private const string _projectWithAccess = "TestProjectWithAccess";
        private const string _projectWithoutAccess = "TestProjectWithoutAccess";

        [TestInitialize]
        public void Setup()
        {
            _currentUserProviderMock = new Mock<ICurrentUserProvider>();

            _projectAccessCheckerMock = new Mock<IProjectAccessChecker>();

            _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(_projectWithoutAccess)).Returns(false);
            _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(_projectWithAccess)).Returns(true);

            var invitationHelperMock = new Mock<IInvitationHelper>();
            invitationHelperMock.Setup(p => p.GetProjectNameAsync(_invitationIdWithAccessToProject))
                .Returns(Task.FromResult(_projectWithAccess));
            invitationHelperMock.Setup(p => p.GetProjectNameAsync(_invitationIdWithoutAccessToProject))
                .Returns(Task.FromResult(_projectWithoutAccess));

            _loggerMock = new Mock<ILogger<AccessValidator>>();

            _dut = new AccessValidator(
                _currentUserProviderMock.Object,
                _projectAccessCheckerMock.Object,
                invitationHelperMock.Object,
                _loggerMock.Object);
        }

        #region Commands

        #region CreateInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnCreateInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new CreateInvitationCommand(
                null,
                null,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectWithAccess,
                DisciplineType.DP,
                null,
                null,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnCreateInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new CreateInvitationCommand(
                null,
                null,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectWithoutAccess,
                DisciplineType.DP,
                null,
                null,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region DeleteAttachmentCommand
        [TestMethod]
        public async Task ValidateAsync_OnDeleteAttachmentCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new DeleteAttachmentCommand(_invitationIdWithAccessToProject, 0, null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnDeleteAttachmentCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new DeleteAttachmentCommand(_invitationIdWithoutAccessToProject, 0, null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region EditInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnEditInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new EditInvitationCommand(
                _invitationIdWithAccessToProject,
                null,
                null,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                DisciplineType.DP,
                null,
                null,
                null,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnEditInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new EditInvitationCommand(
                _invitationIdWithoutAccessToProject,
                null,
                null,
                null,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                DisciplineType.DP,
                null,
                null,
                null,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region UploadAttachmentCommand
        [TestMethod]
        public async Task ValidateAsync_OnUploadAttachmentCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new UploadAttachmentCommand(
                _invitationIdWithAccessToProject,
                null,
                true,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnUploadAttachmentCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new UploadAttachmentCommand(
                _invitationIdWithoutAccessToProject,
                null,
                true,
                null);
            
            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region CompleteInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnCompleteInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new CompletePunchOutCommand(
                _invitationIdWithAccessToProject,
                null,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnCompleteInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new CompletePunchOutCommand(
                _invitationIdWithoutAccessToProject,
                null,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region AcceptInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnAcceptInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new AcceptPunchOutCommand(
                _invitationIdWithAccessToProject,
                null,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnAcceptInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new AcceptPunchOutCommand(
                _invitationIdWithoutAccessToProject,
                null,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region UnAcceptInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnUnAcceptInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new UnAcceptPunchOutCommand(
                _invitationIdWithAccessToProject,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnUnAcceptInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new UnAcceptPunchOutCommand(
                _invitationIdWithoutAccessToProject,
                null,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region SignInvitationCommand
        [TestMethod]
        public async Task ValidateAsync_OnSignInvitationCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new SignPunchOutCommand(
                _invitationIdWithAccessToProject,
                0,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnSignInvitationCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new SignPunchOutCommand(
                _invitationIdWithoutAccessToProject,
                0,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region AddCommentCommand
        [TestMethod]
        public async Task ValidateAsync_OnAddCommentCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new AddCommentCommand(
                _invitationIdWithAccessToProject,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnAddCommentCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new AddCommentCommand(
                _invitationIdWithoutAccessToProject,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region CancelPunchOutCommand
        [TestMethod]
        public async Task ValidateAsync_OnCancelPunchOutCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new CancelPunchOutCommand(
                _invitationIdWithAccessToProject,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnCancelPunchOutCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new CancelPunchOutCommand(
                _invitationIdWithoutAccessToProject,
                null);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region CreateSavedFilterCommand
        [TestMethod]
        public async Task ValidateAsync_OnCreateSavedFilterCommand_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var command = new CreateSavedFilterCommand(
                _projectWithAccess,
                "title",
                "criteria",
                false);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnCreateSavedFilterCommand_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var command = new CreateSavedFilterCommand(
                _projectWithoutAccess,
                "title",
                "criteria",
                false);

            // act
            var result = await _dut.ValidateAsync(command);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #endregion

        #region Queries

        #region GetAttachmentByIdQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetAttachmentByIdQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetAttachmentByIdQuery(_invitationIdWithAccessToProject, 0);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetAttachmentByIdQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetAttachmentByIdQuery(_invitationIdWithoutAccessToProject, 0);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetAttachmentsQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetAttachmentsQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetAttachmentsQuery(_invitationIdWithAccessToProject);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetAttachmentsQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetAttachmentsQuery(_invitationIdWithoutAccessToProject);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetCommPkgsInProjectQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetCommPkgsInProjectQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetCommPkgsInProjectQuery(_projectWithAccess, null, 10, 0);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetCommPkgsInProjectQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetCommPkgsInProjectQuery(_projectWithoutAccess, null, 10, 0);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetInvitationByIdQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationByIdQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetInvitationByIdQuery(_invitationIdWithAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationByIdQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetInvitationByIdQuery(_invitationIdWithoutAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetInvitations
        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetInvitationsQuery(_projectWithAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetInvitationsQuery(_projectWithoutAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetInvitationsByCommPkgNoQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsByCommPkgNoQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetInvitationsByCommPkgNoQuery("commpkg", _projectWithAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsByCommPkgNoQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetInvitationsByCommPkgNoQuery("commpkg", _projectWithoutAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetInvitationsByCommPkgNosQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsByCommPkgNosQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> {"commpkg"}, _projectWithAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetInvitationsByCommPkgNosQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetLatestMdpIpoStatusOnCommPkgsQuery(new List<string> { "commpkg" }, _projectWithoutAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetMcPkgsUnderCommPkgInProjectQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetMcPkgsUnderCommPkgInProjectQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetMcPkgsUnderCommPkgInProjectQuery(_projectWithAccess, null);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetMcPkgsUnderCommPkgInProjectQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetMcPkgsUnderCommPkgInProjectQuery(_projectWithoutAccess, null);
            
            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetHistoryQuery
        [TestMethod]
        public async Task ValidateAsync_OnGetHistoryQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetHistoryQuery(_invitationIdWithAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetHistoryQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetHistoryQuery(_invitationIdWithoutAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetComments
        [TestMethod]
        public async Task ValidateAsync_OnGetCommentsQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetCommentsQuery(_invitationIdWithAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetCommentsQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetCommentsQuery(_invitationIdWithoutAccessToProject);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #region GetSavedFiltersInProject
        [TestMethod]
        public async Task ValidateAsync_OnGetSavedFiltersInProjectQuery_ShouldReturnTrue_WhenAccessToProject()
        {
            // Arrange
            var query = new GetSavedFiltersInProjectQuery(_projectWithAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateAsync_OnGetSavedFiltersInProjectQuery_ShouldReturnFalse_WhenNoAccessToProject()
        {
            // Arrange
            var query = new GetSavedFiltersInProjectQuery(_projectWithoutAccess);

            // act
            var result = await _dut.ValidateAsync(query);

            // Assert
            Assert.IsFalse(result);
        }
        #endregion

        #endregion
    }
}
