﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompleteInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Command.Validators.RowVersionValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CompleteInvitation
{
    [TestClass]
    public class CompleteInvitationCommandValidatorTests
    {
        private CompleteInvitationCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private Mock<IRowVersionValidator> _rowVersionValidatorMock;

        private CompleteInvitationCommand _command;
        private const string _projectName = "Project name";
        private const string _title = "Test title";
        private const string _description = "body";
        private const string _location = "location A";
        private const int _id = 1;
        private const DisciplineType _type = DisciplineType.DP;
        private const string _invitationRowVersion = "AAAAAAAAABA=";
        private const string _participantRowVersion = "AAAAAAAAABA=";

        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
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
                new PersonForCommand(null, "Ola", "Nordman", "ola@test.com", true),
                null,
                1)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _rowVersionValidatorMock = new Mock<IRowVersionValidator>();
            _rowVersionValidatorMock.Setup(r => r.IsValid(_invitationRowVersion)).Returns(true);
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IpoExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.IpoIsInPlannedStageAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ValidContractorParticipantExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(true));
            _command = new CompleteInvitationCommand(
                _id,
                _invitationRowVersion,
                _participantRowVersion);

            _dut = new CompleteInvitationCommandValidator(_invitationValidatorMock.Object, _rowVersionValidatorMock.Object);
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
        public void Validate_ShouldFail_WhenInvitationIsNotInPlannedStage()
        {
            _invitationValidatorMock.Setup(inv => inv.IpoIsInPlannedStageAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Invitation is not in planned stage, and thus cannot be completed"));
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
            _rowVersionValidatorMock.Setup(r => r.IsValid(_participantRowVersion)).Returns(false);

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participant row version is not valid!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenInvitationDoesNotHaveContractorParticipant()
        {
            _invitationValidatorMock.Setup(inv => inv.ContractorExistsAsync(_id, default)).Returns(Task.FromResult(false));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("The IPO does not have a contractor assigned to complete the IPO!"));
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