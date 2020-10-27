using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
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
        private readonly string _projectName = "Project name";
        private readonly string _projectName2 = "Project name 2";
        private readonly string _title1 = "Test title";
        private readonly string _title2 = "Test title 2";
        private int _invitation1Id;
        private int _invitation2Id;
        private int _invitation3Id;
        private readonly DisciplineType _typeDp = DisciplineType.DP;

        private readonly IList<McPkgScopeForCommand> _mcPkgScope = new List<McPkgScopeForCommand>
        {
            new McPkgScopeForCommand("MC01", "D1", "COMM-01")
        };

        private readonly IList<CommPkgScopeForCommand> _commPkgScope = new List<CommPkgScopeForCommand>
        {
            new CommPkgScopeForCommand("COMM-02", "D2", "PA")
        };

        private readonly List<ParticipantsForCommand> _participantsOnlyRequired = new List<ParticipantsForCommand>
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
                new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Ola", "Nordmann", "ola@test.com", true),
                null,
                1)
        };

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(TestPlant, _projectName, _title1, _typeDp);
                context.Invitations.Add(invitation);
                _invitation1Id = invitation.Id;
                var invitation2 = new Invitation(TestPlant, _projectName, _title2, _typeDp);
                context.Invitations.Add(invitation2);
                _invitation2Id = invitation2.Id;
                var invitation3 = new Invitation(TestPlant, _projectName2, _title2, _typeDp);
                context.Invitations.Add(invitation3);
                _invitation3Id = invitation3.Id;
                context.SaveChangesAsync().Wait();
            }
        }


        [TestMethod]
        public async Task TitleExistsOnProjectAsync_SameTitleSameProject_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectAsync(_projectName, _title1, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task TitleExistsOnProjectAsync_SameTitleDifferentProject_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectAsync("newProject", _title1, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task TitleExistsOnProjectAsync_DifferentTitleSameProject_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectAsync(_projectName, "new title", default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_McPkgScopeOnly_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(_mcPkgScope, new List<CommPkgScopeForCommand>());
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnly_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(new List<McPkgScopeForCommand>(), _commPkgScope);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScope_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(_mcPkgScope, _commPkgScope);
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
        public void RequiredParticipantsMustBeInvited_RequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
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
        public void RequiredParticipantsMustBeInvited_OnlyExternalParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor, 
                        new ExternalEmailForCommand("external@test.com"),
                        null,
                        null,
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        new ExternalEmailForCommand("external2@test.com"),
                        null,
                        null,
                        1)
                });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyOneRequiredParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand>{_participantsOnlyRequired[0]});
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
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_ParticipantsWithoutParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidParticipantList(new List<ParticipantsForCommand> {
                    _participantsOnlyRequired[0],
                    _participantsOnlyRequired[1],
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        null,
                        null,
                        3)
                });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_ParticipantsWithMoreThanOneParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidParticipantList(new List<ParticipantsForCommand> {
                    _participantsOnlyRequired[0],
                    _participantsOnlyRequired[1],
                    new ParticipantsForCommand(
                        Organization.Operation,
                        new ExternalEmailForCommand("test@email.com"),
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true),
                        null,
                        3)
                });
                Assert.IsFalse(result);
            }
        }


        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndThreeExtra_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var fr =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", false, null),
                        2);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "Smith", "zoey@test.com", true), 
                        null,
                        3);
                var external =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("External@test.com"),
                        null,
                        null,
                        4);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], fr, person, external });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndPersonWithOnlyGuid_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "Smith", null, true),
                        null,
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndPersonWithOnlyEmail_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true),
                        null,
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_FunctionalRoleUsingGroupEmailInvalidEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRoleWithoutEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "test", false, null),
                        0);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand>{_participantsOnlyRequired[0], _participantsOnlyRequired[1], functionalRoleWithoutEmail});
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_FunctionalRoleUsingGroupEmailMissingEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRoleWithoutEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", null, false, null),
                        0);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], functionalRoleWithoutEmail });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_ParticipantWithFunctionalRoleAndExternalEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("external@test.com"),
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", false, null),
                        0);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], participantWithFunctionalRoleAndExternalEmail });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_PersonMissingEmailAndOid_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", null, true), 
                        null,
                        4);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_PersonInvalidEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "Smith", "test", true),
                        null,
                        4);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_PersonEmptyGuidAndInvalidEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(Guid.Empty, "Zoey", "Smith", "test", true),
                        null,
                        4);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void OnlyRequiredParticipantsHaveLowestSortKeys_OnlyRequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void OnlyRequiredParticipantsHaveLowestSortKeys_AdditionalParticipantsHaveHighSortKey_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true),
                        null,
                        3);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void OnlyRequiredParticipantsHaveLowestSortKeys_RequiredParticipantsHaveWrongSortKey_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr@test.com", false, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new PersonForCommand(null, "Ola", "Nordmann", "ola@test.com", true),
                        null,
                        0)
                });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void OnlyRequiredParticipantsHaveLowestSortKeys_AdditionalParticipantsHaveLowSortKey_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true),
                        null,
                        0);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoTitleExistsInProjectOnAnotherIpoAsync_TitleExistsOnAnotherIpoInProject_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectOnAnotherIpoAsync(_projectName, _title1, _invitation2Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoTitleExistsInProjectOnAnotherIpoAsync_TitleExistsOnAnotherIpoInAnotherProject_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectOnAnotherIpoAsync(_projectName2, _title1, _invitation3Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoTitleExistsInProjectOnAnotherIpoAsync_TitleExistsOnIpoToBeUpdated_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoTitleExistsInProjectOnAnotherIpoAsync(_projectName, _title1, _invitation1Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_ExternalWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("test@email.com", 1),
                        null,
                        null,
                        3);
                var result = dut.ParticipantMustHaveId(externalPerson);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_PersonWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, 1),
                        null,
                        3);
                var result = dut.ParticipantMustHaveId(person);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_FunctionalRoleWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", false, null, 1),
                        0);
                var result = dut.ParticipantMustHaveId(functionalRole);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_FunctionalRoleWithPersonsWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", false, new List<PersonForCommand> {
                            new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, 2),
                            new PersonForCommand(null, "Zoey1", "Smith", "zoey1@test.com", false, 1)
                            }, 
                            1),
                        0);
                var result = dut.ParticipantMustHaveId(functionalRole);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_ExternalEmailWithoutId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("test@email.com"), 
                        null,
                        null,
                        3);
                var result = dut.ParticipantMustHaveId(externalPerson);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_PersonWithoutId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true), 
                        null,
                        3);
                var result = dut.ParticipantMustHaveId(person);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_FunctionalRoleWithoutId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", false, null),
                        0);
                var result = dut.ParticipantMustHaveId(functionalRole);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void ParticipantMustHaveId_FunctionalRoleWithPersonsWithoutId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new FunctionalRoleForCommand("FR1", "fr1@test.com", true, new List<PersonForCommand> { 
                            new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, 2),
                            new PersonForCommand(null, "Zoey1", "Smith", "zoey1@test.com", false)
                            }
                        ),
                        0);
                var result = dut.ParticipantMustHaveId(functionalRole);
                Assert.IsFalse(result);
            }
        }

    }
}
