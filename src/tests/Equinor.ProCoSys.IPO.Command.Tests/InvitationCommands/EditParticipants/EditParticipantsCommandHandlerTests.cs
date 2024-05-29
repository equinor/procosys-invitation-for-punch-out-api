using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditParticipants;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Equinor.ProCoSys.IPO.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.EditParticipants
{
    [TestClass]
    public class EditParticipantsCommandHandlerTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<IIntegrationEventPublisher> _integrationEventPublisherMock;

        private EditParticipantsCommand _command;
        private EditParticipantsCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _participantRowVersion = "AAAAAAAAJ00=";
        private const int _participantId = 20;
        private const string _projectName = "Project name";
        private static readonly Guid _projectGuid = new Guid("11111111-2222-2222-2222-333333333341");
        private readonly Project _project = new(_plant, _projectName, $"Description of {_projectName}", _projectGuid);
        private const string _firstName = "Ola";
        private const string _lastName = "Nordmann";
        private const DisciplineType _typeDp = DisciplineType.DP;
        private Invitation _dpInvitation;
        private const int _dpInvitationId = 60;

        private static Guid _azureOid = new Guid("11111111-1111-2222-3333-333333333333");
        private static Guid _newAzureOid = new Guid("11111111-2222-2222-3333-333333333333");
        private const string _functionalRoleCode = "FR1";
        private const string _newFunctionalRoleCode = "NEWFR1";
        private const string _functionalRoleWithMultipleEmailsCode = "FR2";
        private const string _functionalRoleWithMultipleInformationEmailsCode = "FR3";
        private const string _mcPkgNo1 = "MC1";
        private const string _mcPkgNo2 = "MC2";
        private const string _commPkgNo = "Comm1";
        private const string _systemPathWithSection = "14|1|2";

        private readonly List<ParticipantsForEditCommand> _updatedParticipants = new List<ParticipantsForEditCommand>
        {
            new ParticipantsForEditCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForEditCommand(_participantId, _newFunctionalRoleCode, null, _participantRowVersion),
                0),
            new ParticipantsForEditCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForEditCommand(null, _newAzureOid, true, null),
                null,
                1)
        };

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationEventPublisherMock = new Mock<IIntegrationEventPublisher>();

            //mock person response from main API
            var personDetails = new ProCoSysPerson
            {
                AzureOid = _azureOid.ToString(),
                FirstName = _firstName,
                LastName = _lastName,
                Email = "ola@test.com",
                UserName = "ON"
            };
            var newPersonDetails = new ProCoSysPerson
            {
                AzureOid = _newAzureOid.ToString(),
                FirstName = "Kari",
                LastName = "Nordman",
                Email = "kari@test.com",
                UserName = "KN"
            };
            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _azureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult(personDetails));
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _newAzureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult(newPersonDetails));

            //mock functional role response from main API
            var frDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var newFrDetails = new ProCoSysFunctionalRole
            {
                Code = _newFunctionalRoleCode,
                Description = "FR description2",
                Email = "fr2@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var frMultipleEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleEmailsCode,
                Description = "FR description",
                Email = "fr3@email.com;fr76@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };
            var frMultipleInformationEmailsDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleWithMultipleInformationEmailsCode,
                Description = "FR description",
                Email = "fr4@email.com",
                InformationEmail = "ie@email.com;ie2@email.com",
                Persons = null,
                UsePersonalEmail = false
            };
            IList<ProCoSysFunctionalRole> pcsFrDetails = new List<ProCoSysFunctionalRole> { frDetails };
            IList<ProCoSysFunctionalRole> newPcsFrDetails = new List<ProCoSysFunctionalRole> { newFrDetails };
            IList<ProCoSysFunctionalRole> pcsFrMultipleEmailsDetails = new List<ProCoSysFunctionalRole> { frMultipleEmailsDetails };
            IList<ProCoSysFunctionalRole> pcsFrMultipleInformationEmailsDetails = new List<ProCoSysFunctionalRole> { frMultipleInformationEmailsDetails };
            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(pcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _newFunctionalRoleCode }))
                .Returns(Task.FromResult(newPcsFrDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleEmailsCode }))
                .Returns(Task.FromResult(pcsFrMultipleEmailsDetails));
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleWithMultipleInformationEmailsCode }))
                .Returns(Task.FromResult(pcsFrMultipleInformationEmailsDetails));

            var mcPkgs = new List<McPkg>
            {
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo1, "d", _systemPathWithSection,Guid.Empty, Guid.Empty),
                new McPkg(_plant, _project, _commPkgNo, _mcPkgNo2, "d2", _systemPathWithSection, Guid.Empty, Guid.Empty)
            };
            //create invitation
            _dpInvitation = new Invitation(
                    _plant,
                    _project,
                    "dp title",
                    "dp description",
                    _typeDp,
                    new DateTime(),
                    new DateTime(),
                    null,
                    mcPkgs,
                    null);

            var participant = new Participant(
                _plant,
                Organization.Contractor,
                IpoParticipantType.FunctionalRole,
                _functionalRoleCode,
                null,
                null,
                null,
                null,
                null,
                0);
            participant.SetProtectedIdForTesting(_participantId);
            _dpInvitation.AddParticipant(participant);
            _dpInvitation.AddParticipant(new Participant(
                _plant,
                Organization.ConstructionCompany,
                IpoParticipantType.Person,
                null,
                _firstName,
                _lastName,
                null,
                "ola@test.com",
                _azureOid,
                1));
            _dpInvitation.SetProtectedIdForTesting(_dpInvitationId);

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(_dpInvitationId))
                .Returns(Task.FromResult(_dpInvitation));

            //command
            _command = new EditParticipantsCommand(
                _dpInvitationId,
                _updatedParticipants);

            _dut = new EditParticipantsCommandHandler(
                _invitationRepositoryMock.Object,
                _plantProviderMock.Object,
                _unitOfWorkMock.Object,
                _personApiServiceMock.Object,
                _functionalRoleApiServiceMock.Object,
                _integrationEventPublisherMock.Object);
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldThrowErrorIfSigningParticipantDoesNotHaveCorrectPrivileges()
        {
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(_plant,
                    _newAzureOid.ToString(), "IPO", new List<string> { "SIGN" }))
                .Returns(Task.FromResult<ProCoSysPerson>(null));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Person does not have required privileges to be the"));
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldThrowErrorIfFunctionalRoleDoesNotExistOrHaveIPOClassification()
        {
            IList<ProCoSysFunctionalRole> functionalRoles = new List<ProCoSysFunctionalRole>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(
                    _plant, new List<string> { _newFunctionalRoleCode }))
                .Returns(Task.FromResult(functionalRoles));

            var result = await Assert.ThrowsExceptionAsync<IpoValidationException>(() =>
                _dut.Handle(_command, default));
            Assert.IsTrue(result.Message.StartsWith("Could not find functional role with functional role code"));
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldUpdateParticipants()
        {
            Assert.AreEqual(2, _dpInvitation.Participants.Count);
            Assert.AreEqual(_azureOid, _dpInvitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(_functionalRoleCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);

            await _dut.Handle(_command, default);

            Assert.AreEqual(_newAzureOid, _dpInvitation.Participants.ToList()[1].AzureOid);
            Assert.AreEqual(_newFunctionalRoleCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldSeVersion()
        {
            // Act
            var result = await _dut.Handle(_command, default);

            // Assert
            // In real life EF Core will create a new RowVersion when save.
            // Since UnitOfWorkMock is a Mock this will not happen here, so we assert that RowVersion is set from command
            Assert.IsTrue(_dpInvitation.Participants.Any(p => p.RowVersion.ConvertToString() == _participantRowVersion));
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInEmailField()
        {
            // Setup
            var newParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(null, _functionalRoleWithMultipleEmailsCode, null, _participantRowVersion),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(null, _azureOid, true, null),
                    null,
                    1)
            };

            var command = new EditParticipantsCommand(
                _dpInvitationId,
                newParticipants);

            await _dut.Handle(command, default);

            // Assert
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(_functionalRoleWithMultipleEmailsCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }

        [TestMethod]
        public async Task HandlingUpdateParticipantsCommand_ShouldNotFailWhenAFunctionalRoleHasMultipleEmailsInInformationEmailField()
        {
            // Setup
            var newParticipants = new List<ParticipantsForEditCommand>
            {
                new ParticipantsForEditCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForEditCommand(null, _functionalRoleWithMultipleInformationEmailsCode, null, _participantRowVersion),
                    0),
                new ParticipantsForEditCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForEditCommand(null, _azureOid, true, null),
                    null,
                    1)
            };

            var command = new EditParticipantsCommand(
                _dpInvitationId,
                newParticipants);

            await _dut.Handle(command, default);

            // Assert
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(_functionalRoleWithMultipleInformationEmailsCode, _dpInvitation.Participants.ToList()[0].FunctionalRoleCode);
        }
    }
}
