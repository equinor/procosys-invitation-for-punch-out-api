using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Fusion.Integration.Meeting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandHandlerFusionFailsTests
    {
        private Mock<IPlantProvider> _plantProviderMock;
        private Mock<IFusionMeetingClient> _meetingClientMock;
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<ICommPkgApiService> _commPkgApiServiceMock;
        private Mock<IMcPkgApiService> _mcPkgApiServiceMock;
        private Mock<IPersonApiService> _personApiServiceMock;
        private Mock<IFunctionalRoleApiService> _functionalRoleApiServiceMock;
        private Mock<IOptionsMonitor<MeetingOptions>> _meetingOptionsMock;
        private Mock<IDbContextTransaction> _transactionMock;

        private const string _functionalRoleCode = "FR1";
        private static Guid _azureOid = new Guid("11111111-1111-2222-2222-333333333333");

        private readonly string _plant = "PCS$TEST_PLANT";
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new FunctionalRoleForCommand(_functionalRoleCode, null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new PersonForCommand(_azureOid,  "Ola", "Nordman", "ola@test.com", true),
                null,
                1)
        };

        private ProCoSysPerson _personDetails;
        private ProCoSysFunctionalRole _functionalRoleDetails;

        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _description = "Body";
        private readonly string _location = "Outside";
        private readonly DisciplineType _type = DisciplineType.DP;

        private CreateInvitationCommandHandler _dut;
        private CreateInvitationCommand _command;

        [TestInitialize]
        public void Setup()
        {
            _plantProviderMock = new Mock<IPlantProvider>();
            _plantProviderMock
                .Setup(x => x.Plant)
                .Returns(_plant);

            _meetingClientMock = new Mock<IFusionMeetingClient>();
            _meetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Throws(new Exception("Something failed"));

            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.Add(It.IsAny<Invitation>()));

            _transactionMock = new Mock<IDbContextTransaction>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _unitOfWorkMock.Setup(x => x.BeginTransaction(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_transactionMock.Object));

            _commPkgApiServiceMock = new Mock<ICommPkgApiService>();

            _mcPkgApiServiceMock = new Mock<IMcPkgApiService>();

            _personDetails = new ProCoSysPerson
            {
                AzureOid = _azureOid.ToString(),
                FirstName = "Ola",
                LastName = "Nordman",
                Email = "ola@test.com"
            };

            _personApiServiceMock = new Mock<IPersonApiService>();
            _personApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    _plant,
                    _azureOid.ToString(),
                    "IPO",
                    new List<string>{ "CREATE", "SIGN" }))
                .Returns(Task.FromResult(_personDetails));

            _functionalRoleDetails = new ProCoSysFunctionalRole
            {
                Code = _functionalRoleCode,
                Description = "FR description",
                Email = "fr@email.com",
                InformationEmail = null,
                Persons = null,
                UsePersonalEmail = false
            };

            IList<ProCoSysFunctionalRole> frDetails = new List<ProCoSysFunctionalRole>{ _functionalRoleDetails };

            _functionalRoleApiServiceMock = new Mock<IFunctionalRoleApiService>();
            _functionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(_plant, new List<string> { _functionalRoleCode }))
                .Returns(Task.FromResult(frDetails));

            _meetingOptionsMock = new Mock<IOptionsMonitor<MeetingOptions>>();

            _command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                null);

            _dut = new CreateInvitationCommandHandler(
                _plantProviderMock.Object,
                _meetingClientMock.Object,
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _commPkgApiServiceMock.Object,
                _mcPkgApiServiceMock.Object,
                _personApiServiceMock.Object,
                _functionalRoleApiServiceMock.Object,
                _meetingOptionsMock.Object);
        }

        [TestMethod]
        public async Task HandleCreateInvitationCommand_ShouldRollbackIfFusionApiFails()
        {
            await _dut.Handle(_command, default);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _transactionMock.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
