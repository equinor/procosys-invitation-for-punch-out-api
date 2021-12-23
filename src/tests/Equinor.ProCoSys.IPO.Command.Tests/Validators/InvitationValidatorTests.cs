using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
        private const string _title4 = "Test title 4";
        private const string _title5 = "Test title 5";
        private const string _title6 = "Test title 6";
        private int _invitationIdWithFrAsParticipants;
        private int _invitationIdWithoutParticipants;
        private int _invitationIdWithCurrentUserOidAsParticipants;
        private int _invitationIdWithNotCurrentUserOidAsParticipants;
        private int _invitationIdWithValidAndNonValidSignerParticipants;
        private int _invitationIdWithAnotherCreator;
        private int _participantId1;
        private int _participantId2;
        private int _operationCurrentPersonId;
        private int _operationFrId;
        private int _operationNotCurrentPersonId;
        private int _contractorId;
        private int _commissioningId;
        private int _additionalContractorId;
        private int _supplierId;
        private const string _description = "Test description";
        private const DisciplineType _typeDp = DisciplineType.DP;
        private const DisciplineType _typeMdp = DisciplineType.MDP;
        protected readonly Guid _azureOid = new Guid("11111111-2222-2222-2222-333333333334");

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
                new EditFunctionalRoleForCommand(1, "FR1", null, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new EditPersonForCommand(2, new Guid("11111111-2222-2222-2222-333333333333"), "ola@test.com", true, null),
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
             var invitationWithCurrentUserAsParticipants = new Invitation(
                    TestPlant,
                    _projectName,
                    _title1,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2")},
                    null);

                foreach (var attachment in _attachments)
                {
                    invitationWithCurrentUserAsParticipants.AddAttachment(attachment);
                }
                context.Invitations.Add(invitationWithCurrentUserAsParticipants);
                _invitationIdWithCurrentUserOidAsParticipants = invitationWithCurrentUserAsParticipants.Id;
                var participant1 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "First1",
                    "Last",
                    "UN1",
                    "first1@last.com",
                    _currentUserOid,
                    0);
                var participant2 = new Participant(TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "First2",
                    "Last",
                    "UN2",
                    "first2@last.com",
                    _currentUserOid,
                    1);
                var participant3 = new Participant(
                    TestPlant,
                    Organization.Operation,
                    IpoParticipantType.Person,
                    null,
                    "First3",
                    "Last",
                    "first3@last.com",
                    "UN3",
                    _currentUserOid,
                    2);

                invitationWithCurrentUserAsParticipants.AddParticipant(participant1);
                invitationWithCurrentUserAsParticipants.AddParticipant(participant2);
                invitationWithCurrentUserAsParticipants.AddParticipant(participant3);

                var currentPerson = context.Persons.SingleAsync(p => p.Oid == _currentUserOid).Result;

                invitationWithCurrentUserAsParticipants.CompleteIpo(
                    participant1,
                    participant1.RowVersion.ConvertToString(),
                    currentPerson,
                    new DateTime());

                invitationWithCurrentUserAsParticipants.AcceptIpo(
                    participant2,
                    participant2.RowVersion.ConvertToString(),
                    currentPerson,
                    new DateTime());

                var invitationWithFrAsParticipants = new Invitation(
                    TestPlant,
                    _projectName,
                    _title2,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2")},
                    null);
                context.Invitations.Add(invitationWithFrAsParticipants);
                _invitationIdWithFrAsParticipants = invitationWithFrAsParticipants.Id;
                var frContractor = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.FunctionalRole,
                    "FR code",
                    null,
                    null,
                    null,
                    "fr@test.com",
                    null,
                    0);
                var frConstructionCompany = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.FunctionalRole,
                    "FR code 2",
                    null,
                    null,
                    null,
                    "fr2@test.com",
                    null,
                    1);
                var frOperation = new Participant(
                    TestPlant,
                    Organization.Operation,
                    IpoParticipantType.FunctionalRole,
                    "FR code op",
                    null,
                    null,
                    null,
                    "op@test.com",
                    null,
                    2);
                invitationWithFrAsParticipants.AddParticipant(frContractor);
                invitationWithFrAsParticipants.AddParticipant(frConstructionCompany);
                invitationWithFrAsParticipants.AddParticipant(frOperation);

                var invitationWithoutParticipants = new Invitation(
                    TestPlant,
                    _projectName2,
                    _title3,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    null);
                context.Invitations.Add(invitationWithoutParticipants);
                _invitationIdWithoutParticipants = invitationWithoutParticipants.Id;

                var invitationWithNotCurrentUserAsParticipants = new Invitation(
                    TestPlant,
                    _projectName2,
                    _title4,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    null);

                context.Invitations.Add(invitationWithNotCurrentUserAsParticipants);
                _invitationIdWithNotCurrentUserOidAsParticipants = invitationWithNotCurrentUserAsParticipants.Id;
                var contractorParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "First1",
                    "Last",
                    "UN1",
                    "first1@last.com",
                    _azureOid,
                    0);
                var constructionParticipant = new Participant(TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "First2",
                    "Last",
                    "UN2",
                    "first2@last.com",
                    _azureOid,
                    1);
                var operationParticipant = new Participant(TestPlant,
                    Organization.Operation,
                    IpoParticipantType.Person,
                    null,
                    "First3",
                    "Last",
                    "UN3",
                    "first3@last.com",
                    _azureOid,
                    2);
                invitationWithNotCurrentUserAsParticipants.AddParticipant(contractorParticipant);
                invitationWithNotCurrentUserAsParticipants.AddParticipant(constructionParticipant);
                invitationWithNotCurrentUserAsParticipants.AddParticipant(operationParticipant);

                var invitationWithValidAndNonValidSignerParticipants = new Invitation(
                    TestPlant,
                    _projectName,
                    _title6,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                    null);

                context.Invitations.Add(invitationWithValidAndNonValidSignerParticipants);
                _invitationIdWithValidAndNonValidSignerParticipants = invitationWithValidAndNonValidSignerParticipants.Id;
                var contractorParticipant2 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "First1",
                    "Last",
                    "UN1",
                    "first1@last.com",
                    _currentUserOid,
                    0);
                var constructionParticipant2 = new Participant(
                    TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "First2",
                    "Last",
                    "UN2",
                    "first2@last.com",
                    _currentUserOid,
                    1);
                var commissioningParticipant = new Participant(
                    TestPlant,
                    Organization.Commissioning,
                    IpoParticipantType.Person,
                    null,
                    "First3",
                    "Last",
                    "UN3",
                    "first3@last.com",
                    _currentUserOid,
                    2);
                var additionalContractorParticipant = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "First4",
                    "Last",
                    "UN4",
                    "first4@last.com",
                    _currentUserOid,
                    3);
                var supplierParticipant = new Participant(TestPlant,
                    Organization.Supplier,
                    IpoParticipantType.Person,
                    null,
                    "First5",
                    "Last",
                    "UN5",
                    "first5@last.com",
                    _currentUserOid,
                    4);
                invitationWithValidAndNonValidSignerParticipants.AddParticipant(contractorParticipant2);
                invitationWithValidAndNonValidSignerParticipants.AddParticipant(constructionParticipant2);
                invitationWithValidAndNonValidSignerParticipants.AddParticipant(commissioningParticipant);
                invitationWithValidAndNonValidSignerParticipants.AddParticipant(additionalContractorParticipant);
                invitationWithValidAndNonValidSignerParticipants.AddParticipant(supplierParticipant);

                context.SaveChangesAsync().Wait();

                // Add invitation with another currentuserprovider
                {
                    var tempCurrentUserProviderMock = new Mock<ICurrentUserProvider>();
                    var tempUserOid = new Guid("99999999-9999-9999-9999-999999999999");
                    tempCurrentUserProviderMock.Setup(x => x.GetCurrentUserOid()).Returns(tempUserOid);
                    var tempcurrentUserProvider = tempCurrentUserProviderMock.Object;

                    // ensure current user exists in db
                    using (var context2 = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, tempcurrentUserProvider))
                    {
                        if (context2.Persons.SingleOrDefault(p => p.Oid == tempUserOid) == null)
                        {
                            AddPerson(context2, tempUserOid, "Another", "User", "au", "au@pcs.pcs");
                        }
                    }

                    using (var context2 = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, tempcurrentUserProvider))
                    {
                        var invitationWithAnotherCreator = new Invitation(
                        TestPlant,
                        _projectName,
                        _title5,
                        _description,
                        _typeDp,
                        new DateTime(),
                        new DateTime(),
                        null,
                        new List<McPkg> { new McPkg(TestPlant, _projectName, "Comm", "Mc", "d", "1|2") },
                        null);
                        context2.Invitations.Add(invitationWithAnotherCreator);

                        context2.SaveChangesAsync().Wait();
                        _invitationIdWithAnotherCreator = invitationWithAnotherCreator.Id;
                    }
                }

                _participantId1 = participant1.Id;
                _participantId2 = participant2.Id;
                _operationCurrentPersonId = participant3.Id;
                _operationFrId = frOperation.Id;
                _operationNotCurrentPersonId = operationParticipant.Id;
                _contractorId = contractorParticipant2.Id;
                _commissioningId = commissioningParticipant.Id;
                _additionalContractorId = additionalContractorParticipant.Id;
                _supplierId = supplierParticipant.Id;
            }
        }

        [TestMethod]
        public void IsValidScope_McPkgScopeOnlyOnDp_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeDp, _mcPkgScope, new List<string>());
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnlyOnMdp_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeMdp, new List<string>(), _commPkgScope);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_McPkgScopeOnlyOnMdp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeMdp, _mcPkgScope, new List<string>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnlyOnDp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeDp, new List<string>(), _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScopeOnDP_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeDp, _mcPkgScope, _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScopeOnMDP_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeMdp, _mcPkgScope, _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_NoScopeTypeDp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeDp,new List<string>(), new List<string>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_NoScopeTypeMdp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidScope(_typeMdp, new List<string>(), new List<string>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_RequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_NoParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyExternalParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        new EditExternalEmailForCommand(null, "external@test.com", null),
                        null,
                        null,
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        new EditExternalEmailForCommand(null, "external2@test.com", null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> { _participantsOnlyRequired[0] });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyParticipantsNotRequiredInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(null, "FR1", null, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new EditPersonForCommand(null, Guid.Empty, "ola@test.com", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_ParticipantsWithoutParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.IsValidParticipantList(new List<ParticipantsForCommand> {
                    _participantsOnlyRequired[0],
                    _participantsOnlyRequired[1],
                    new ParticipantsForCommand(
                        Organization.Operation,
                        new EditExternalEmailForCommand(null, "test@email.com", null),
                        new EditPersonForCommand(null, null, "zoey@test.com", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var fr =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(null, "FR1", null, null),
                        2);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new EditPersonForCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), "zoey@test.com", true, null),
                        null,
                        3);
                var external =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new EditExternalEmailForCommand(null, "External@test.com", null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new EditPersonForCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), null, true, null),
                        null,
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndPersonWithGuidAndEmptyEmail_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new EditPersonForCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), "", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new EditPersonForCommand(null, null, "zoey@test.com", true, null),
                        null,
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndFRWithPersonWithGuidAndEmptyEmail_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var fr = new ParticipantsForCommand(
                    Organization.Operation,
                    null,
                    null,
                    new EditFunctionalRoleForCommand(
                        null, 
                        "FR",
                        new List<EditPersonForCommand>{ new EditPersonForCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), "", true, null)},
                        null), 
                    3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], fr });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_ParticipantWithFunctionalRoleAndExternalEmail_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new EditExternalEmailForCommand(null, "external@test.com", null),
                        null,
                        new EditFunctionalRoleForCommand(null, "FR1", null, null),
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], participantWithFunctionalRoleAndExternalEmail });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_FunctionalRoleWithNullAsCode_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(null, null, null, null),
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], participantWithFunctionalRoleAndExternalEmail });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_FunctionalRoleWithEmptyCode_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(null, "", null, null),
                        3);
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(null, null, null, true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(null, Guid.Empty, "test", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(null, null, "zoey@test.com", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(null, "FR1", null, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new EditPersonForCommand(null, null, "ola@test.com", true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(null, null, "zoey@test.com", true, null),
                        null,
                        0);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_ExternalParticipantHasIdAndExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new EditExternalEmailForCommand(_participantId1, "test@email.com", null),
                        null,
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(_participantId1, null, "zoey@test.com", true, null),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(_participantId1, "FR1", null, null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithPersonsWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(
                            _operationCurrentPersonId, 
                            "FR1", 
                        new List<EditPersonForCommand> {
                            new EditPersonForCommand(_participantId1, null, "zoey@test.com", true, null),
                            new EditPersonForCommand(_participantId2, null, "zoey1@test.com", false, null)
                            },
                            null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_ExternalEmailWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new EditExternalEmailForCommand(200, "test@email.com", null),
                        null,
                        null,
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new EditPersonForCommand(500, null, "zoey@test.com", true, null),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(400, "FR1", null, null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantMustHaveId_FunctionalRoleWithPersonsWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new EditFunctionalRoleForCommand(
                            null, 
                            "FR1", 
                            new List<EditPersonForCommand> {
                                new EditPersonForCommand(400, null, "zoey@test.com", true, null),
                                new EditPersonForCommand(500, null, "zoey1@test.com", false, null)
                            },
                            null
                        ),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoExistsAsync_ExistingInvitationId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoExistsAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoExistsAsync_NonExistingInvitationId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoExistsAsync(100, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ValidAccepterParticipantExistsAsync_FunctionalRoleAsAccepter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidAccepterParticipantExistsAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidAccepterParticipantExistsAsync_PersonAsAccepter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidAccepterParticipantExistsAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidAccepterParticipantExistsAsync_AccepterPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidAccepterParticipantExistsAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoHasAccepterAsync_AccepterExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoHasAccepterAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoHasAccepterAsync_AccepterDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoHasAccepterAsync(_invitationIdWithoutParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ValidCompleterParticipantExistsAsyncAsync_FunctionalRoleAsCompleter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidCompleterParticipantExistsAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidCompleterParticipantExistsAsync_PersonAsCompleter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidCompleterParticipantExistsAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidCompleterParticipantExistsAsync_CompleterPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidCompleterParticipantExistsAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoHasCompleterAsync_CompleterExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoHasCompleterAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoHasCompleterAsync_CompleterDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoHasCompleterAsync(_invitationIdWithoutParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_FunctionalRoleAsSigner_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidSigningParticipantExistsAsync(_invitationIdWithFrAsParticipants, _operationFrId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_PersonAsSigner_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidSigningParticipantExistsAsync(_invitationIdWithCurrentUserOidAsParticipants, _operationCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_SignerPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidSigningParticipantExistsAsync(_invitationIdWithNotCurrentUserOidAsParticipants, _operationNotCurrentPersonId, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_ShouldThrowException_WhenSignerIsContractor()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.ValidSigningParticipantExistsAsync(_invitationIdWithValidAndNonValidSignerParticipants, _contractorId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_SignerIsCommissioning_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidSigningParticipantExistsAsync(_invitationIdWithValidAndNonValidSignerParticipants, _commissioningId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_SignerIsAdditionalContractor_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.ValidSigningParticipantExistsAsync(_invitationIdWithValidAndNonValidSignerParticipants, _additionalContractorId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_ShouldThrowException_WhenSignerIsSupplier()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.ValidSigningParticipantExistsAsync(_invitationIdWithValidAndNonValidSignerParticipants, _supplierId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }

        [TestMethod]
        public async Task SignerExistsAsync_SignerExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.SignerExistsAsync(_invitationIdWithCurrentUserOidAsParticipants, _operationCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SignerExistsAsync_SignerDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.SignerExistsAsync(_invitationIdWithoutParticipants, _operationCurrentPersonId, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsCreatorOfInvitation_CurrentUserIsCreator_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.CurrentUserIsCreatorOfInvitation(_invitationIdWithoutParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsCreatorOfInvitation_CurrentUserIsNotCreator_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.CurrentUserIsCreatorOfInvitation(_invitationIdWithAnotherCreator, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInPlannedStage_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithFrAsParticipants, IpoStatus.Planned, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInAcceptedStage_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipants, IpoStatus.Accepted, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInPlannedStage_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipants, IpoStatus.Planned, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInCompletedStage_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipants, IpoStatus.Completed, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SameUserUnAcceptingThatAcceptedAsync_SameUser_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.SameUserUnAcceptingThatAcceptedAsync(_invitationIdWithCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SameUserUnAcceptingThatAcceptedAsync_DifferentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider);
                var result = await dut.SameUserUnAcceptingThatAcceptedAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                //This is not a full test coverage, because we do not have a history event for this accepting. We get false because there are not history events in this validation. Cannot add history event that is created by a user other than current user
                Assert.IsFalse(result);
            }
        }
    }
}
