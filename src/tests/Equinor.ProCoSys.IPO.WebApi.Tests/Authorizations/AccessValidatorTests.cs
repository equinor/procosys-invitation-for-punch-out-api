using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
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
        private const int InvitationIdWithAccessToProject = 1;
        private const int InvitationIdWithoutAccessToProject = 2;
        private const string ProjectWithAccess = "TestProjectWithAccess";
        private const string ProjectWithoutAccess = "TestProjectWithoutAccess";

        [TestInitialize]
        public void Setup()
        {
            _currentUserProviderMock = new Mock<ICurrentUserProvider>();

            _projectAccessCheckerMock = new Mock<IProjectAccessChecker>();

            _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(ProjectWithoutAccess)).Returns(false);
            _projectAccessCheckerMock.Setup(p => p.HasCurrentUserAccessToProject(ProjectWithAccess)).Returns(true);

            var invitationHelperMock = new Mock<IInvitationHelper>();
            invitationHelperMock.Setup(p => p.GetProjectNameAsync(InvitationIdWithAccessToProject)).Returns(Task.FromResult(ProjectWithAccess));
            invitationHelperMock.Setup(p => p.GetProjectNameAsync(InvitationIdWithoutAccessToProject)).Returns(Task.FromResult(ProjectWithoutAccess));

            _loggerMock = new Mock<ILogger<AccessValidator>>();

            _dut = new AccessValidator(
                _currentUserProviderMock.Object,
                _projectAccessCheckerMock.Object,
                invitationHelperMock.Object,
                _loggerMock.Object);
        }

        // todo Add tests for each Query / Command with security checks. See preservation solution
    }
}
