using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.Validators
{
    [TestClass]
    public class InvitationValidatorTests : ReadOnlyTestsBase
    {
        private readonly string projectName = "Project name";
        private readonly string title = "Test title";
        private readonly string body = "body";
        private readonly string _ocation = "location A";
        private readonly DisciplineType typeDP = DisciplineType.DP;
        private readonly DisciplineType typeMDP = DisciplineType.MDP;

        private readonly IList<McPkgScopeForCommand> mcPkgScope = new List<McPkgScopeForCommand>
        {
            new McPkgScopeForCommand("MC01", "D1", "COMM-01")
        };

        private readonly IList<CommPkgScopeForCommand> commPkgScope = new List<CommPkgScopeForCommand>
        {
            new CommPkgScopeForCommand("COMM-02", "D2", "PA")
        };

        private readonly List<ParticipantsForCommand> participantsOnlyRequired = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand("FR1", "fr@test.com", false, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(Guid.Empty, "Ola", "Nordmann", "ola@test.com", true),
                null,
                1)
        };

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(TestPlant, projectName, title, typeDP);
                context.Invitations.Add(invitation);
                context.SaveChangesAsync().Wait();
            }
        }


        [TestMethod]
        public async Task TitleExistsOnProjectAsync_SameTitleSameProject_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.TitleExistsOnProjectAsync(projectName, title, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task TitleExistsOnProjectAsync_SameTitleDifferentProject_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.TitleExistsOnProjectAsync("newProject", title, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task TitleExistsOnProjectAsync_DifferentTitleSameProject_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.TitleExistsOnProjectAsync(projectName, "new title", default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScope_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(mcPkgScope, commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_NoScope_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(new List<McPkgScopeForCommand>(), new List<CommPkgScopeForCommand>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_McPkgScopeOnly_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(mcPkgScope, new List<CommPkgScopeForCommand>());
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnly_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(new List<McPkgScopeForCommand>(), commPkgScope);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_RequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_NoParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyOneRequiredParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand>{participantsOnlyRequired[0]});
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyParticipantsNotRequiredInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr@test.com", false, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new PersonForCommand(Guid.Empty, "Ola", "Nordmann", "ola@test.com", true),
                        null,
                        1)
                });
                Assert.IsFalse(result); 
            }
        }

        [TestMethod]
        public void IsValidParticipantList_OnlyRequiredParticipantsList_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        //[TestMethod]
        //public void IsValidParticipantList_FunctionalRoleWithUseGroupdEmailMissingEmail_ReturnsFalse()
        //{
        //    using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
        //    {
        //        var dut = new InvitationValidator(context);
        //        var functionalRoleWithoutEmail = 
        //            new ParticipantsForCommand(
        //                Organization.Commissioning,
        //                null,
        //                null,
        //                new FunctionalRoleForCommand("FR1", "test", false, null),
        //                0);
        //        var result = dut.RequiredParticipantsMustBeInvited(participantsOnlyRequired.Append(functionalRoleWithoutEmail));
        //        Assert.IsFalse(result);
        //    }
        //}
    }
}
