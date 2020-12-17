using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.Tests.Misc
{
    [TestClass]
    public class InvitationHelperTests : ReadOnlyTestsBase
    {
        private int _invitationId;
        private const string _projectName = "Project1";

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(
                    TestPlant,
                    _projectName,
                    "Title",
                    "Description",
                    DisciplineType.DP,
                    new DateTime(),
                    new DateTime(),
                    null);
                context.Invitations.Add(invitation);
                context.SaveChangesAsync().Wait();
                _invitationId = invitation.Id;
            }
        }

        [TestMethod]
        public async Task GetProjectName_KnownInvitationId_ShouldReturnProjectName()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                // Arrange
                var dut = new InvitationHelper(context);

                // Act
                var projectName = await dut.GetProjectNameAsync(_invitationId);

                // Assert
                Assert.AreEqual(_projectName, projectName);
            }
        }

        [TestMethod]
        public async Task GetProjectName_UnKnownInvitationId_ShouldReturnNull()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                // Arrange
                var dut = new InvitationHelper(context);

                // Act
                var projectName = await dut.GetProjectNameAsync(0);

                // Assert
                Assert.IsNull(projectName);
            }
        }
    }
}
