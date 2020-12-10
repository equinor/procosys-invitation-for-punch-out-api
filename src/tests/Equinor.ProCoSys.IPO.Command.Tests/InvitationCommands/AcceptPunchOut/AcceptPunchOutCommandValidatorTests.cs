using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.AcceptPunchOut
{
    [TestClass]
    public class AcceptPunchOutCommandValidatorTests
    {
        private AcceptPunchOutCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private AcceptPunchOutCommand _command;
        private const int _id = 1;
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";
        private const string _note = "note A";
        private const int _participantId1 = 10;
        private const int _participantId2 = 20;
        private const string _participantRowVersion1 = "AAAAAAAAABB=";
        private const string _participantRowVersion2 = "AAAAAAAAABM=";
        private readonly List<UpdateNoteOnParticipantForCommand> _participants = new List<UpdateNoteOnParticipantForCommand>
        {
            new UpdateNoteOnParticipantForCommand(
                _participantId1,
                _note,
                _participantRowVersion1),
            new UpdateNoteOnParticipantForCommand(
                _participantId2,
                _note,
                _participantRowVersion2)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion1)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion2)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ValidConstructionCompanyParticipantExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ConstructionCompanyExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId1, _id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ParticipantExistsAsync(_participantId2, _id, default)).Returns(Task.FromResult(true));
            _command = new AcceptPunchOutCommand(
                _id,
                _invitationRowVersion,
                _participantRowVersion,
                _participants);

            _dut = new AcceptPunchOutCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
        public void Validate_ShouldFail_WhenInvitationIsNotInCompletedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInStageAsync(_id, IpoStatus.Completed, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is not in completed stage, and thus cannot be accepted"));
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
        public void Validate_ShouldFail_WhenInvitationDoesNotHaveConstructionCompanyParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ConstructionCompanyExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The IPO does not have a construction company assigned to accept the IPO!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenPersonTryingToAcceptIsNotAValidConstructionCompanyParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ValidConstructionCompanyParticipantExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage
                .StartsWith("Person signing is not the construction company assigned to accept this IPO, or there is not a valid construction company on the IPO!"));
        }
    }
}
