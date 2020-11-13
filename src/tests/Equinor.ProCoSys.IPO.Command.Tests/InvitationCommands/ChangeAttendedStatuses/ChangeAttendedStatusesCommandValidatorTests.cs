using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatus;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.ChangeAttendedStatuses;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.ChangeAttendedStatuses
{
    [TestClass]
    public class ChangeAttendedStatusesCommandValidatorTests
    {
        private ChangeAttendedStatusesCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private ChangeAttendedStatusesCommand _command;
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "body";
        private const string _location = "location A";
        private const int _id = 1;
        private const int _participantId1 = 10;
        private const int _participantId2 = 20;
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";

        private readonly List<ParticipantToChangeAttendedStatusForCommand> _participants = new List<ParticipantToChangeAttendedStatusForCommand>
        {
            new ParticipantToChangeAttendedStatusForCommand(
                _participantId1,
                true,
                _participantRowVersion1),
            new ParticipantToChangeAttendedStatusForCommand(
                _participantId2,
                true,
                _participantRowVersion2)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion2)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ValidContractorParticipantExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId1, _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId2, _id, default)).Returns(Task.FromResult(true));
            _command = new ChangeAttendedStatusesCommand(
                _id,
                _invitationRowVersion,
                _participants);

            _dut = new ChangeAttendedStatusesCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("IPO with this ID does not exist!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantIdIsNonExisting()
        {
            _invitationValidatorMock.Setup(inv => inv.ParticipantExists(_participantId1, _id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant with ID does not exist on invitation"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationIsNotInCompletedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is not in completed stage, and thus cannot change attended statuses"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(false);
            
            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation row version is not valid!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantRowVersionIsInvalid()
        {
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant doesn't have valid rowVersion"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationDoesNotHaveContractorParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The IPO does not have a contractor assigned to the IPO!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenPersonTryingToCompleteIsNotAValidContractorParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ValidContractorParticipantExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("Person signing is not the contractor assigned to complete this IPO, or there is not a valid functional role on the IPO!"));
        }
    }
}
