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
        private const string _projectName = "Project name";
        private const string _projectName2 = "Project name 2";
        private const string _title1 = "Test title";
        private const string _title2 = "Test title 2";
        private const string _title3 = "Test title 3";
        private int _invitation1Id;
        private int _invitation2Id;
        private int _invitation3Id;
        private int _participantId1;
        private int _participantId2;
        private int _participantId3;
        private const string _description = "Test description";
        private const DisciplineType _typeDp = DisciplineType.DP;

        private readonly IList<string> _mcPkgScope = new List<string>
        {
            "MC01"
        };

        private readonly IList<string> _commPkgScope = new List<string>
        {
            "COMM-02"
        };

        private readonly List<ParticipantsForCommand> _participantsOnlyRequired = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand("FR1", null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Ola", "Nordmann", "ola@test.com", true),
                null,
                1)
        };

        private readonly List<Attachment> _attachments = new List<Attachment>
        {
            new Attachment(TestPlant, "File1.txt")
        };

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = new Invitation(TestPlant, _projectName, _title1, _description, _typeDp);
                foreach (var attachment in _attachments)
                {
                    invitation.AddAttachment(attachment);
                }
                context.Invitations.Add(invitation);
                _invitation1Id = invitation.Id;
                var participant1 = new Participant(TestPlant, Organization.Contractor, IpoParticipantType.Person, null, "First1", "Last", "UN1", "first1@last.com", null, 0);
                var participant2 = new Participant(TestPlant, Organization.ConstructionCompany, IpoParticipantType.Person, null, "First2", "Last", "UN2", "first2@last.com", null, 1);
                var participant3 = new Participant(TestPlant, Organization.Supplier, IpoParticipantType.Person, null, "First3", "Last", "first3@last.com", "UN3", null, 2);
                invitation.AddParticipant(participant1);
                invitation.AddParticipant(participant2);
                invitation.AddParticipant(participant3);
                
                var invitation2 = new Invitation(TestPlant, _projectName, _title2, _description, _typeDp);
                context.Invitations.Add(invitation2);
                _invitation2Id = invitation2.Id;
                var invitation3 = new Invitation(TestPlant, _projectName2, _title3, _description, _typeDp);
                context.Invitations.Add(invitation3);
                _invitation3Id = invitation3.Id;
                context.SaveChangesAsync().Wait();
                _participantId1 = participant1.Id;
                _participantId2 = participant2.Id;
                _participantId3 = participant3.Id;
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
                var result = dut.IsValidScope(_mcPkgScope, new List<string>());
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnly_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = dut.IsValidScope(new List<string>(), _commPkgScope);
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
                var result = dut.IsValidScope(new List<string>(), new List<string>());
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
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> { _participantsOnlyRequired[0] });
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
                        new FunctionalRoleForCommand("FR1", null),
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
                        new FunctionalRoleForCommand("FR1", null),
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
                        new FunctionalRoleForCommand("FR1", null),
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
                        new FunctionalRoleForCommand("FR1", null),
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
        public async Task ParticipantWithIdExistsAsync_ExternalParticipantHasIdAndExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("test@email.com", _participantId1),
                        null,
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitation1Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, _participantId1),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitation1Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdExists_ReturnsTrue()
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
                        new FunctionalRoleForCommand("FR1", null, _participantId1),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitation1Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithPersonsWithId_ReturnsTrue()
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
                        new FunctionalRoleForCommand("FR1", new List<PersonForCommand> {
                            new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, _participantId1),
                            new PersonForCommand(null, "Zoey1", "Smith", "zoey1@test.com", false, _participantId2)
                            },
                            _participantId3),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitation1Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_ExternalEmailWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new ExternalEmailForCommand("test@email.com", 200), 
                        null,
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitation1Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, 500),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitation1Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdDoesntExist_ReturnsFalse()
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
                        new FunctionalRoleForCommand("FR1", null, 400),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitation1Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantMustHaveId_FunctionalRoleWithPersonsWithIdDoesntExist_ReturnsFalse()
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
                        new FunctionalRoleForCommand("FR1", new List<PersonForCommand> {
                            new PersonForCommand(null, "Zoey", "Smith", "zoey@test.com", true, 400),
                            new PersonForCommand(null, "Zoey1", "Smith", "zoey1@test.com", false, 500)
                            }
                        ),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitation1Id, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoExistsAsync_ExistingInvitationId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoExistsAsync(_invitation1Id, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoExistsAsync_NonExistingInvitationId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context);
                var result = await dut.IpoExistsAsync(100, default);
                Assert.IsFalse(result);
            }
        }

    }
}
