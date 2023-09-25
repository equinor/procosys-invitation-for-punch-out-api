using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.Infrastructure;
using Equinor.ProCoSys.IPO.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.Validators
{
    [TestClass]
    public class InvitationValidatorTests : ReadOnlyTestsBaseInMemory
    {
        #region Setup
        private static readonly Project _project1 = new(TestPlant, _projectName, $"Description of {_projectName} project");
        private static readonly Project _project2 = new(TestPlant, _projectName2, $"Description of {_projectName2} project");
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
        private int _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus;
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
        private int _contractorFrId;
        private int _constructionCompanyFrId;
        private const string _description = "Test description";
        private const DisciplineType _typeDp = DisciplineType.DP;
        private const DisciplineType _typeMdp = DisciplineType.MDP;
        protected readonly Guid _azureOid = new Guid("11111111-2222-2222-2222-333333333334");
        protected IPermissionCache _permissionCacheForAdmin;
        protected Mock<IPermissionCache> _permissionCacheForAdminMock;

        private readonly IList<string> _mcPkgScope = new List<string>
        {
            "MC01"
        };

        private readonly IList<string> _commPkgScope = new List<string>
        {
            "COMM-02"
        };

        private readonly List<ParticipantsForEditCommand> _participantsForEditOnlyRequired = new List<ParticipantsForEditCommand>
        {
            new ParticipantsForEditCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForEditCommand(1, "FR1", null, null),
                0),
            new ParticipantsForEditCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForEditCommand(2, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
                null,
                1)
        };

        private List<ParticipantsForCommand> _participantsOnlyRequired;

        private readonly List<Attachment> _attachments = new List<Attachment>
        {
            new Attachment(TestPlant, "File1.txt")
        };

        protected override void SetupNewDatabase(DbContextOptions<IPOContext> dbContextOptions)
        {
            _participantsOnlyRequired = _participantsForEditOnlyRequired.Cast<ParticipantsForCommand>().ToList();
            _permissionCacheForAdminMock = new Mock<IPermissionCache>();
            IList<string> permissions = new List<string> { "IPO/ADMIN" };
            _permissionCacheForAdminMock.Setup(i => i.GetPermissionsForUserAsync(
                TestPlant, CurrentUserOid))
                .Returns(Task.FromResult(permissions));
            _permissionCacheForAdmin = _permissionCacheForAdminMock.Object;
            using (var context = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitationWithCurrentUserAsParticipants = new Invitation(
                       TestPlant,
                       _project1,
                       _title1,
                       _description,
                       _typeDp,
                       new DateTime(),
                       new DateTime(),
                       null,
                       new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
                       null);

                foreach (var attachment in _attachments)
                {
                    invitationWithCurrentUserAsParticipants.AddAttachment(attachment);
                }
                context.Invitations.Add(invitationWithCurrentUserAsParticipants);
                _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus = invitationWithCurrentUserAsParticipants.Id;
                var participant1 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    null,
                    "First1",
                    "Last",
                    "UN1",
                    "first1@last.com",
                    CurrentUserOid,
                    0);
                var participant2 = new Participant(TestPlant,
                    Organization.ConstructionCompany,
                    IpoParticipantType.Person,
                    null,
                    "First2",
                    "Last",
                    "UN2",
                    "first2@last.com",
                    CurrentUserOid,
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
                    CurrentUserOid,
                    2);

                invitationWithCurrentUserAsParticipants.AddParticipant(participant1);
                invitationWithCurrentUserAsParticipants.AddParticipant(participant2);
                invitationWithCurrentUserAsParticipants.AddParticipant(participant3);

                var currentPerson = context.Persons.SingleAsync(p => p.Guid == CurrentUserOid).Result;

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
                    _project1,
                    _title2,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
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
                    _project2,
                    _title3,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
                    null);
                context.Invitations.Add(invitationWithoutParticipants);
                _invitationIdWithoutParticipants = invitationWithoutParticipants.Id;

                var invitationWithNotCurrentUserAsParticipants = new Invitation(
                    TestPlant,
                    _project2,
                    _title4,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
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
                    _project1,
                    _title6,
                    _description,
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
                    null);

                context.Invitations.Add(invitationWithValidAndNonValidSignerParticipants);
                _invitationIdWithValidAndNonValidSignerParticipants = invitationWithValidAndNonValidSignerParticipants.Id;
                var contractorParticipant2 = new Participant(
                    TestPlant,
                    Organization.Contractor,
                    IpoParticipantType.Person,
                    "Contractor",
                    "First1",
                    "Last",
                    "UN1",
                    "first1@last.com",
                    CurrentUserOid,
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
                    CurrentUserOid,
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
                    CurrentUserOid,
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
                    CurrentUserOid,
                    3);
                var supplierParticipant = new Participant(TestPlant,
                    Organization.Supplier,
                    IpoParticipantType.Person,
                    null,
                    "First5",
                    "Last",
                    "UN5",
                    "first5@last.com",
                    CurrentUserOid,
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
                        if (context2.Persons.SingleOrDefault(p => p.Guid == tempUserOid) == null)
                        {
                            AddPerson(context2, tempUserOid, "Another", "User", "au", "au@pcs.pcs");
                        }
                    }

                    using (var context2 = new IPOContext(dbContextOptions, _plantProvider, _eventDispatcher, tempcurrentUserProvider))
                    {
                        var invitationWithAnotherCreator = new Invitation(
                        TestPlant,
                        _project1,
                        _title5,
                        _description,
                        _typeDp,
                        new DateTime(),
                        new DateTime(),
                        null,
                        new List<McPkg> { new McPkg(TestPlant, _project1, "Comm", "Mc", "d", "1|2") },
                        null);
                        context2.Invitations.Add(invitationWithAnotherCreator);

                        context2.SaveChangesAsync().Wait();
                        _invitationIdWithAnotherCreator = invitationWithAnotherCreator.Id;
                        invitationWithAnotherCreator.AddParticipant(contractorParticipant2);
                        context2.SaveChangesAsync().Wait();
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
                _contractorFrId = frContractor.Id;
                _constructionCompanyFrId = frConstructionCompany.Id;
            }
        }

        #endregion

        #region IsValidScope
        [TestMethod]
        public void IsValidScope_McPkgScopeOnlyOnDp_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeDp, _mcPkgScope, new List<string>());
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnlyOnMdp_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeMdp, new List<string>(), _commPkgScope);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidScope_McPkgScopeOnlyOnMdp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeMdp, _mcPkgScope, new List<string>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgScopeOnlyOnDp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeDp, new List<string>(), _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScopeOnDP_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeDp, _mcPkgScope, _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_CommPkgAndMcPkgScopeOnMDP_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeMdp, _mcPkgScope, _commPkgScope);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_NoScopeTypeDp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeDp, new List<string>(), new List<string>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void IsValidScope_NoScopeTypeMdp_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidScope(_typeMdp, new List<string>(), new List<string>());
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region RequiredParticipantsMustBeInvited
        [TestMethod]
        public void RequiredParticipantsMustBeInvited_RequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_NoParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand>());
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyExternalParticipantsInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        new InvitedExternalEmailForEditCommand(null, "external@test.com", null),
                        null,
                        null,
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        new InvitedExternalEmailForEditCommand(null, "external2@test.com", null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> { _participantsOnlyRequired[0] });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyParticipantsNotRequiredInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, "FR1", null, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new InvitedPersonForEditCommand(null, Guid.Empty, true, null),
                        null,
                        1)
                });
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void RequiredParticipantsMustBeInvited_OnlyRequiredParticipantsList_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.RequiredParticipantsMustBeInvited(_participantsOnlyRequired);
                Assert.IsTrue(result);
            }
        }
        #endregion

        #region IsValidParticipantList
        [TestMethod]
        public void IsValidParticipantList_ParticipantsWithoutParticipantInvited_ReturnsFalse()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.IsValidParticipantList(new List<ParticipantsForCommand> {
                    _participantsOnlyRequired[0],
                    _participantsOnlyRequired[1],
                    new ParticipantsForCommand(
                        Organization.Operation,
                        new InvitedExternalEmailForEditCommand(null, "test@email.com", null),
                        new InvitedPersonForEditCommand(null, new Guid(), true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var fr =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, "FR1", null, null),
                        2);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
                        null,
                        3);
                var external =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new InvitedExternalEmailForEditCommand(null, "External@test.com", null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Operation,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
                        null,
                        3);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void IsValidParticipantList_RequiredParticipantsListAndFRWithPersonWithGuid_ReturnsTrue()
        {
            using (var context = new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var fr = new ParticipantsForCommand(
                    Organization.Operation,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(
                        null,
                        "FR",
                        new List<InvitedPersonForEditCommand> { new InvitedPersonForEditCommand(null, new Guid("11111111-2222-2222-2222-333333333333"), true, null) },
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new InvitedExternalEmailForEditCommand(null, "external@test.com", null),
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, "FR1", null, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, null, null, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var participantWithFunctionalRoleAndExternalEmail =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, "", null, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid(), true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(null, Guid.Empty, true, null),
                        null,
                        4);
                var result = dut.IsValidParticipantList(
                    new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region OnlyRequiredParticipantsHaveLowestSortKeys
        [TestMethod]
        public void OnlyRequiredParticipantsHaveLowestSortKeys_OnlyRequiredParticipantsInvited_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid(), true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(null, "FR1", null, null),
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid(), true, null),
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(null, new Guid(), true, null),
                        null,
                        0);
                var result = dut.OnlyRequiredParticipantsHaveLowestSortKeys(new List<ParticipantsForCommand> { _participantsOnlyRequired[0], _participantsOnlyRequired[1], person });
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region ParticipantWithIdExists
        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_ExternalParticipantHasIdAndExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new InvitedExternalEmailForEditCommand(_participantId1, "test@email.com", null),
                        null,
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(_participantId1, new Guid(), true, null),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(_participantId1, "FR1", null, null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithPersonsWithId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(
                            _operationCurrentPersonId,
                            "FR1",
                        new List<InvitedPersonForEditCommand> {
                            new InvitedPersonForEditCommand(_participantId1, new Guid(), true, null),
                            new InvitedPersonForEditCommand(_participantId2, new Guid(), false, null)
                            },
                            null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_ExternalEmailWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var externalPerson =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        new InvitedExternalEmailForEditCommand(200, "test@email.com", null),
                        null,
                        null,
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(externalPerson, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_PersonWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var person =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        new InvitedPersonForEditCommand(500, new Guid(), true, null),
                        null,
                        3);
                var result = await dut.ParticipantWithIdExistsAsync(person, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(400, "FR1", null, null),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task ParticipantWithIdExistsAsync_FunctionalRoleWithPersonsWithIdDoesntExist_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var functionalRole =
                    new ParticipantsForCommand(
                        Organization.Commissioning,
                        null,
                        null,
                        new InvitedFunctionalRoleForEditCommand(
                            null,
                            "FR1",
                            new List<InvitedPersonForEditCommand> {
                                new InvitedPersonForEditCommand(400, new Guid(), true, null),
                                new InvitedPersonForEditCommand(500, new Guid(), false, null)
                            },
                            null
                        ),
                        0);
                var result = await dut.ParticipantWithIdExistsAsync(functionalRole, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region IpoExists
        [TestMethod]
        public async Task IpoExistsAsync_ExistingInvitationId_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoExistsAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoExistsAsync_NonExistingInvitationId_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoExistsAsync(100, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsValidAccepterParticipant
        [TestMethod]
        public async Task CurrentUserIsValidAccepterParticipantAsync_FunctionalRoleAsAccepter_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code 2"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidAccepterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidAccepterParticipantAsync_PersonNotInFunctionalRole_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidAccepterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidAccepterParticipantAsync_PersonAsAccepter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidAccepterParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidAccepterParticipantAsync_AccepterPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidAccepterParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region IpoHasAccepter
        [TestMethod]
        public async Task IpoHasAccepterAsync_AccepterExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoHasAccepterAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoHasAccepterAsync_AccepterDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoHasAccepterAsync(_invitationIdWithoutParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region ParticipantExists
        [TestMethod]
        public async Task ParticipantExistsAsync_ParticipantExists_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.ParticipantExistsAsync(_participantId1, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }
        
        [TestMethod]
        public async Task ParticipantExistsAsync_ParticipantDoesNotExistOnInvitation_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.ParticipantExistsAsync(_participantId1, _invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region BeSignedParticipant

        [TestMethod]
        public async Task BeSignedParticipantAsync_ParticipantIsSigned_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.ParticipantIsSignedAsync(_participantId2, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }
        
        [TestMethod]
        public async Task BeSignedParticipantAsync_ParticipantIsNotSigned_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.ParticipantIsSignedAsync(_operationCurrentPersonId, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsValidCompleterParticipant
        [TestMethod]
        public async Task CurrentUserIsValidCompleterParticipantAsync_FunctionalRoleAsCompleter_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidCompleterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidCompleterParticipantAsync_PersonNotInFunctionalRole_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidCompleterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidCompleterParticipantAsync_PersonAsCompleter_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidCompleterParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidCompleterParticipantAsync_CompleterPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidCompleterParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region IpoHasCompleter
        [TestMethod]
        public async Task IpoHasCompleterAsync_CompleterExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoHasCompleterAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoHasCompleterAsync_CompleterDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoHasCompleterAsync(_invitationIdWithoutParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsValidSigningParticipant
        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_UserIsNotInFunctionalRole_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithFrAsParticipants, _operationFrId, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_FunctionalRoleAsSigner_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code op"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson { AzureOid = CurrentUserOid.ToString() }));
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithFrAsParticipants, _operationFrId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_PersonAsSigner_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, _operationCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_SignerPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, _operationNotCurrentPersonId, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_ShouldThrowException_WhenSignerIsFirstContractor()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _contractorId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_SignerIsCommissioning_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _commissioningId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsValidSigningParticipantAsync_SignerIsAdditionalContractor_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _additionalContractorId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task ValidSignerParticipantExistsAsync_ShouldThrowException_WhenSignerIsSupplier()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.CurrentUserIsValidSigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _supplierId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }
        #endregion

        #region CurrentUserIsAdminOrValidUnsigningParticipant
        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_UserNotInFunctionalRole_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithFrAsParticipants, _operationFrId, default);
                Assert.IsFalse(result);
            }
        }
        
        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_FunctionalRoleAsUnsigner_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code op"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson { AzureOid = CurrentUserOid.ToString() }));
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithFrAsParticipants, _operationFrId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_PersonAsUnsigner_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, _operationCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_UnsignerPersonIsntCurrentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, _operationNotCurrentPersonId, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_ShouldThrowException_WhenUnsignerIsContractor()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _contractorId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_UnsignerIsCommissioning_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _commissioningId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_UnsignerIsAdmin_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCacheForAdmin);
                var result = await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, _operationNotCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidUnsigningParticipantAsync_ShouldThrowException_WhenUnsignerIsSupplier()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
                    await dut.CurrentUserIsAdminOrValidUnsigningParticipantAsync(_invitationIdWithValidAndNonValidSignerParticipants, _supplierId, default)
                );
                Assert.AreEqual(result.Message, "Sequence contains no elements");
            }
        }
        #endregion

        #region SignerExists
        [TestMethod]
        public async Task SignerExistsAsync_SignerExists_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SignerExistsAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, _operationCurrentPersonId, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SignerExistsAsync_SignerDoesntExists_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SignerExistsAsync(_invitationIdWithoutParticipants, _operationCurrentPersonId, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsAllowedToCancelIpo
        [TestMethod]
        public async Task CurrentUserIsAllowedToCancelIpo_CurrentUserIsCreator_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToCancelIpoAsync(_invitationIdWithoutParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToCancelIpo_CurrentUserIsNotCreator_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToCancelIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsFalse(result);
            }
        }


        [TestMethod]
        public async Task CurrentUserIsAllowedToCancelIpo_CurrentUserIsAdmin_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCacheForAdmin);
                var result = await dut.CurrentUserIsAllowedToCancelIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToCancelIpo_CurrentUserIsNotCreatorOfInvitationButContractor_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                TestPlant,
                CurrentUserOid.ToString(),
                "Contractor"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToCancelIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToCancelIpo_CurrentUserIsNotCreatorOfInvitationAndNotContractor_ReturnsFalse()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(TestPlant, CurrentUserOid.ToString(), "Contractor")).Returns(Task.FromResult<ForeignApi.ProCoSysPerson>(null));

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToCancelIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsAllowedToDeleteIpo
        [TestMethod]
        public async Task CurrentUserIsAllowedToDeleteIpo_CurrentUserIsCreator_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToDeleteIpoAsync(_invitationIdWithoutParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToDeleteIpo_CurrentUserIsNotCreator_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToDeleteIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToDeleteIpo_CurrentUserIsAdmin_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCacheForAdmin);
                var result = await dut.CurrentUserIsAllowedToDeleteIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAllowedToDeleteIpo_CurrentUserIsNotCreatorOfInvitationButContractor_ReturnsFalse()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                TestPlant,
                CurrentUserOid.ToString(),
                "Contractor"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAllowedToDeleteIpoAsync(_invitationIdWithAnotherCreator, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region IpoIsInStage
        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInPlannedStage_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
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
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, IpoStatus.Accepted, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInPlannedStage_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, IpoStatus.Planned, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task IpoIsInStageAsync_IpoIsInCompletedStage_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.IpoIsInStageAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, IpoStatus.Completed, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsAdminOrValidCompletorParticipant
        [TestMethod]
        public async Task CurrentUserIsAdminOrValidCompletorParticipantAsync_UserIsAdmin_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCacheForAdmin);
                var result = await dut.CurrentUserIsAdminOrValidCompletorParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidCompletorParticipantAsync_SameUser_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidCompletorParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidCompletorParticipantAsync_DifferentUser_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidCompletorParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidCompletorParticipantAsync_FunctionalRoleAsCompleter_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidCompletorParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidCompletorParticipantAsync_NotInFr_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidCompletorParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region CurrentUserIsAdminOrValidAcceptorParticipant
        [TestMethod]
        public async Task CurrentUserIsAdminOrValidAccepterParticipantAsync_UserIsAdmin_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCacheForAdmin);
                var result = await dut.CurrentUserIsAdminOrValidAccepterParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidAccepterParticipantAsync_SameUser_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidAccepterParticipantAsync(_invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidAccepterParticipantAsync_DifferentUser_ReturnsFalse()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidAccepterParticipantAsync(_invitationIdWithNotCurrentUserOidAsParticipants, default);
                //This is not a full test coverage, because we do not have a history event for this accepting. We get false because there are not history events in this validation. Cannot add history event that is created by a user other than current user
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidAccepterParticipantAsync_FunctionalRoleAsAccepter_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code 2"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidAccepterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task CurrentUserIsAdminOrValidAccepterParticipantAsync_PersonNotInFunctionalRole_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.CurrentUserIsAdminOrValidAccepterParticipantAsync(_invitationIdWithFrAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region HasPermissionToEditParticipant
        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_ParticipantIsCurrentUser_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_operationCurrentPersonId, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_ParticipantHasSigned_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_participantId2, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_ParticipantHasSignedButIsAdmin_ReturnsTrue()
        {
            var permissionCacheMock = new Mock<IPermissionCache>();
            IList<string> ipoAdminPrivilege = new List<string> { "IPO/ADMIN" };
            permissionCacheMock
                .Setup(x => x.GetPermissionsForUserAsync(_plantProvider.Plant, CurrentUserOid))
                .Returns(Task.FromResult(ipoAdminPrivilege));
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, permissionCacheMock.Object);
                var result = await dut.HasPermissionToEditParticipantAsync(_participantId2, _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_UserIsIpoAdmin_ReturnsTrue()
        {
            var permissionCacheMock = new Mock<IPermissionCache>();
            IList<string> ipoAdminPrivilege = new List<string> { "IPO/ADMIN" };
            permissionCacheMock
                .Setup(x => x.GetPermissionsForUserAsync(_plantProvider.Plant, CurrentUserOid))
                .Returns(Task.FromResult(ipoAdminPrivilege));
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, permissionCacheMock.Object);
                var result = await dut.HasPermissionToEditParticipantAsync(_operationNotCurrentPersonId, _invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_UserIsNotIpoAdmin_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_operationNotCurrentPersonId, _invitationIdWithNotCurrentUserOidAsParticipants, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_UserIsFirstContractor_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_contractorFrId, _invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_UserIsFirstConstructionCompany_ReturnsTrue()
        {
            _personApiServiceMock.Setup(i => i.GetPersonInFunctionalRoleAsync(
                    TestPlant,
                    CurrentUserOid.ToString(),
                    "FR code 2"))
                .Returns(Task.FromResult(new ForeignApi.ProCoSysPerson() { AzureOid = CurrentUserOid.ToString() }));
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_constructionCompanyFrId, _invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasPermissionToEditParticipantAsync_UserIsNotInContractorFr_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasPermissionToEditParticipantAsync(_contractorFrId, _invitationIdWithFrAsParticipants, default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region HasOppositeAttendedStatusIfTouched
        [TestMethod]
        public async Task HasOppositeAttendedStatusAsync_ParticipantHasOppositeAttendedStatusIfTouched_NotTouched_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasOppositeAttendedStatusIfTouchedAsync(_contractorFrId, _invitationIdWithFrAsParticipants, true, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasOppositeAttendedStatusAsync_ParticipantHasOppositeAttendedStatusIfTouched_NotTouched_ReturnsTrue2()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasOppositeAttendedStatusIfTouchedAsync(_contractorFrId, _invitationIdWithFrAsParticipants, false, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task HasOppositeAttendedStatusAsync_ParticipantDoesNotHaveOppositeAttendedStatusIfTouched_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations.Include(inv => inv.Participants).Single(inv => inv.Id == _invitationIdWithFrAsParticipants);
                var participant = invitation.Participants.Single(p => p.Id == _contractorFrId);
                invitation.UpdateAttendedStatus(participant, false, participant.RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasOppositeAttendedStatusIfTouchedAsync(_contractorFrId, _invitationIdWithFrAsParticipants, false, default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task HasOppositeAttendedStatusAsync_ParticipantHasOppositeAttendedStatusIfTouched_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations.Include(inv => inv.Participants).Single(inv => inv.Id == _invitationIdWithFrAsParticipants);
                var participant = invitation.Participants.Single(p => p.Id == _contractorFrId);
                invitation.UpdateAttendedStatus(participant, false, participant.RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.HasOppositeAttendedStatusIfTouchedAsync(_contractorFrId, _invitationIdWithFrAsParticipants, true, default);
                Assert.IsTrue(result);
            }
        }
        #endregion

        #region SortKeyCannotBeChangedForSignedFirstSignersAsync
        [TestMethod]
        public async Task SortKeyCannotBeChangedForSignedFirstSignersAsync_PlannedStatus_ReturnsTrue()
        {
            using (var context =
                new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SortKeyCannotBeChangedForSignedFirstSignersAsync(_participantsForEditOnlyRequired, _invitationIdWithAnotherCreator, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SortKeyCannotBeChangedForSignedFirstSignersAsync_CompletedStatus_WrongId_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;

                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithFrAsParticipants);
                invitation.CompleteIpo(
                    invitation.Participants.First(),
                    invitation.Participants.First().RowVersion.ConvertToString(),
                    person,
                    DateTime.Now);
                context.SaveChangesAsync().Wait();
            }

            var updatedParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(1, "FR code", null, null),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(2, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
                    null,
                    1)
            };
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SortKeyCannotBeChangedForSignedFirstSignersAsync(
                    updatedParticipants,
                    _invitationIdWithFrAsParticipants,
                    default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SortKeyCannotBeChangedForSignedFirstSignersAsync_CompletedStatus_CorrectId_ReturnsTrue()
        {
            int contractorId;
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;

                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithFrAsParticipants);
                invitation.CompleteIpo(
                    invitation.Participants.First(),
                    invitation.Participants.First().RowVersion.ConvertToString(),
                    person,
                    DateTime.Now);
                contractorId = invitation.Participants.First().Id;
                context.SaveChangesAsync().Wait();
            }

            var updatedParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(contractorId, "FR code", null, null),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(2, new Guid("11111111-2222-2222-2222-333333333333"), true, null),
                    null,
                    1)
            };
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SortKeyCannotBeChangedForSignedFirstSignersAsync(updatedParticipants, _invitationIdWithFrAsParticipants, default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SortKeyCannotBeChangedForSignedFirstSignersAsync_CompletedStatus_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                invitation.UnAcceptIpo(
                    invitation.Participants.First(),
                    invitation.Participants.First().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SortKeyCannotBeChangedForSignedFirstSignersAsync(
                    _participantsForEditOnlyRequired,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SortKeyCannotBeChangedForSignedFirstSignersAsync_AcceptedStatus_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SortKeyCannotBeChangedForSignedFirstSignersAsync(
                    _participantsForEditOnlyRequired,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsFalse(result);
            }
        }
        #endregion

        #region SignedParticipantsCannotBeAlteredAsync
        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_NoSignatures_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                invitation.UnAcceptIpo(
                    invitation.Participants.Single(p => p.SortKey == 1),
                    invitation.Participants.Single(p => p.SortKey == 1).RowVersion.ConvertToString());
                invitation.UnCompleteIpo(
                    invitation.Participants.First(),
                    invitation.Participants.First().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                _participantsForEditOnlyRequired.Add(
                    new ParticipantsForEditCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForEditCommand(null, _azureOid, true, null),
                        null,
                        3));
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    _participantsForEditOnlyRequired,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_ChangeFirstParticipantsWithSignatures_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    _participantsForEditOnlyRequired,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_SignedButNotChangedParticipants_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;

                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                invitation.SignIpo(
                    invitation.Participants.Last(),
                    person,
                    invitation.Participants.Last().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                Assert.IsTrue(invitation.Participants.All(p => p.SignedBy != null));

                var participantsToUpdate = new List<ParticipantsForEditCommand>();
                foreach (var participant in invitation.Participants)
                {
                    // We know that there are only three person participants
                    var person = new InvitedPersonForEditCommand(participant.Id, (Guid) participant.AzureOid, true, participant.RowVersion.ConvertToString());
                    var participantsForEditCommand = new ParticipantsForEditCommand(participant.Organization, null, person, null,
                        participant.SortKey);
                    participantsToUpdate.Add(participantsForEditCommand);
                }
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                _participantsForEditOnlyRequired.Add(
                    new ParticipantsForEditCommand
                        (Organization.ConstructionCompany,
                            null,
                            new InvitedPersonForEditCommand(null, _azureOid, true, null),
                            null,
                            3));
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    participantsToUpdate,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_RemovingSignedParticipant_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;

                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                invitation.SignIpo(
                    invitation.Participants.Last(),
                    person,
                    invitation.Participants.Last().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                Assert.IsTrue(invitation.Participants.All(p => p.SignedBy != null));

                var participantsToUpdate = new List<ParticipantsForEditCommand>();
                foreach (var participant in invitation.Participants)
                {
                    // We know that there are only three person participants
                    var person = new InvitedPersonForEditCommand(participant.Id, (Guid) participant.AzureOid, true, participant.RowVersion.ConvertToString());
                    var participantsForEditCommand = new ParticipantsForEditCommand(participant.Organization, null, person, null,
                        participant.SortKey);
                    if (participant.SortKey < 2)
                    {
                        participantsToUpdate.Add(participantsForEditCommand);
                    }
                }
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
   
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    participantsToUpdate,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_SignedButNotChangedParticipants_AddNew_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                var participantsToUpdate = new List<ParticipantsForEditCommand>();
                foreach (var participant in invitation.Participants)
                {
                    // We know that there are only three person participants
                    var person = new InvitedPersonForEditCommand(participant.Id, (Guid) participant.AzureOid, true, participant.RowVersion.ConvertToString());
                    var participantsForEditCommand = new ParticipantsForEditCommand(participant.Organization, null, person, null,
                        participant.SortKey);
                    participantsToUpdate.Add(participantsForEditCommand);
                }
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                _participantsForEditOnlyRequired.Add(
                    new ParticipantsForEditCommand
                        (Organization.ConstructionCompany,
                            null,
                            new InvitedPersonForEditCommand(null, _azureOid, true, null),
                            null,
                            3));
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    participantsToUpdate,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_ChangingSignedParticipantToExternal_ReturnsFalse()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var person = context.QuerySet<Person>().FirstAsync().Result;

                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                invitation.SignIpo(
                    invitation.Participants.Last(),
                    person,
                    invitation.Participants.Last().RowVersion.ConvertToString());
                context.SaveChangesAsync().Wait();
            }

            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                var participantsToUpdate = new List<ParticipantsForEditCommand>();
                foreach (var participant in invitation.Participants)
                {
                    // We know that there are only three person participants
                    var person = new InvitedPersonForEditCommand(participant.Id, (Guid) participant.AzureOid, true, participant.RowVersion.ConvertToString());
                    var participantsForEditCommand = new ParticipantsForEditCommand(participant.Organization, null, person, null,
                        participant.SortKey);
                    if (participant.SortKey > 1)
                    {
                        var external = new InvitedExternalEmailForEditCommand(participant.Id, "email", participant.RowVersion.ConvertToString());
                        participantsForEditCommand = new ParticipantsForEditCommand(Organization.External, external, null, null, participant.SortKey);
                    }
                    participantsToUpdate.Add(participantsForEditCommand);
                }
                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                _participantsForEditOnlyRequired.Add(
                    new ParticipantsForEditCommand
                        (Organization.ConstructionCompany,
                            new InvitedExternalEmailForEditCommand(null, "email", null),
                            null,
                            null,
                            3));
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    participantsToUpdate,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public async Task SignedParticipantsCannotBeAlteredAsync_SignedButChangeOnlyUnsignedParticipants_ReturnsTrue()
        {
            using (var context =
                   new IPOContext(_dbContextOptions, _plantProvider, _eventDispatcher, _currentUserProvider))
            {
                var invitation = context.Invitations
                    .Include(i => i.Participants)
                    .Single(inv => inv.Id == _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus);
                var participantsToUpdate = new List<ParticipantsForEditCommand>();
                foreach (var participant in invitation.Participants)
                {
                    // We know that there are only three person participants
                    var person = new InvitedPersonForEditCommand(participant.Id, (Guid) participant.AzureOid, true, participant.RowVersion.ConvertToString());
                    var participantsForEditCommand = new ParticipantsForEditCommand(participant.Organization, null, person, null,
                        participant.SortKey);
                    if (participant.SortKey > 1)
                    {
                        person = new InvitedPersonForEditCommand(null, _azureOid, true, null);
                        participantsForEditCommand = new ParticipantsForEditCommand(Organization.Commissioning, null, person, null, 11);
                    }
                    participantsToUpdate.Add(participantsForEditCommand);
                }

                var dut = new InvitationValidator(context, _currentUserProvider, _personApiService, _plantProvider, _permissionCache);
                _participantsForEditOnlyRequired.Add(
                    new ParticipantsForEditCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForEditCommand(null, _azureOid, true, null),
                        null,
                        3));
                var result = await dut.SignedParticipantsCannotBeAlteredAsync(
                    participantsToUpdate,
                    _invitationIdWithCurrentUserOidAsParticipantsAndAcceptedStatus,
                    default);
                Assert.IsTrue(result);
            }
        }
        #endregion
    }
}
