using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeletePunchOut;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.DeletePunchOut
{
    [TestClass]
    public class DeletePunchOutCommandHandlerTests
    {
        private Mock<IInvitationRepository> _invitationRepositoryMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IHistoryRepository> _historyRepositoryMock;

        private DeletePunchOutCommand _command;
        private DeletePunchOutCommandHandler _dut;
        private const string _plant = "PCS$TEST_PLANT";
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "Test description";
        private const DisciplineType _typeDP = DisciplineType.DP;
        private readonly Guid _meetingId = new Guid("11111111-2222-2222-2222-333333333333");
        private static Guid _azureOidForCurrentUser = new Guid("11111111-1111-2222-3333-333333333334");
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private Invitation _invitation;

        [TestInitialize]
        public void Setup()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            var currentPerson = new Person(_azureOidForCurrentUser, null, null, null, null);
            //create invitation
            _invitation = new Invitation(
                    _plant,
                    _projectName,
                    _title,
                    _description,
                    _typeDP,
                    new DateTime(),
                    new DateTime(),
                    null,
                    new List<McPkg> { new McPkg(_plant, _projectName, "Comm", "Mc", "d", "1|2")},
                    null)
                { MeetingId = _meetingId };


            _invitationRepositoryMock = new Mock<IInvitationRepository>();
            _invitationRepositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(_invitation));

            var history = new List<History>
            {
                new History(_plant, "description", _invitation.ObjectGuid, EventType.IpoCanceled)
            };
            _historyRepositoryMock = new Mock<IHistoryRepository>();
            _historyRepositoryMock
                .Setup(x => x.GetHistoryByObjectGuid(_invitation.ObjectGuid))
                .Returns(history);

            _invitation.CancelIpo(currentPerson);

            //command
            _command = new DeletePunchOutCommand(_invitation.Id, _invitationRowVersion);

            _dut = new DeletePunchOutCommandHandler(
                _invitationRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _historyRepositoryMock.Object);
        }

        // This test does not offer a lot of coverage except for that changes should be saved.
        [TestMethod]
        public async Task DeletePunchOutCommand_ShouldDeletePunchOut()
        {
            Assert.AreEqual(IpoStatus.Canceled, _invitation.Status);

            var result = await _dut.Handle(_command, default);

            Assert.AreEqual(ServiceResult.ResultType.Ok, result.ResultType);
            _unitOfWorkMock.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
