using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class CreateInvitationCommandValidatorTests
    {
        private CreateInvitationCommandValidator _dut;
        private Mock<IInvitationValidator> _invitationValidatorMock;
        private CreateInvitationCommand _command;
        private readonly string _projectName = "Project name";
        private readonly string _title = "Test title";
        private readonly string _description = "body";
        private readonly string _location = "location A";
        private readonly DisciplineType _type = DisciplineType.DP;

        private readonly IList<string> _commPkgScope = new List<string> {"COMM-02"};
        private readonly List<ParticipantsForCommand> _participants = new List<ParticipantsForCommand>
        {
            new ParticipantsForCommand(
                Organization.Contractor,
                null,
                null,
                new InvitedFunctionalRoleForCreateCommand("FR1", null),
                0),
            new ParticipantsForCommand(
                Organization.ConstructionCompany,
                null,
                new InvitedPersonForCreateCommand(new Guid(), true),
                null,
                1)
        };

        [TestInitialize]
        public void Setup_OkState()
        {
            _invitationValidatorMock = new Mock<IInvitationValidator>();
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(_type, new List<string>(), _commPkgScope)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(true);
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
                _commPkgScope,
                false);

            _dut = new CreateInvitationCommandValidator(_invitationValidatorMock.Object);
        }

        [TestMethod]
        public async Task Validate_ShouldBeValid_WhenOkState()
        {
            var result = await _dut.ValidateAsync(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipantsIsNull()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                null,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Participants must be invited!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenAnyParticipantHaveInvalidEmail()
        {
            var participants = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new InvitedFunctionalRoleForCreateCommand("FR1", null),
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new InvitedPersonForCreateCommand(new Guid(), true),
                    null,
                    1),
                new ParticipantsForCommand(
                    Organization.External,
                    new InvitedExternalEmailForCreateCommand("abc.example.com"),
                    null,
                    null,
                    -3)
            };

            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);

            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("One, or more, of the email-addresses for the participants is invalid."));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenProjectNameIsTooShort()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                "p",
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project name must be between 3 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenProjectNameIsTooLong()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                new string('x', Invitation.ProjectNameMaxLength + 1),
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project name must be between 3 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenProjectNameIsNull()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                null,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Project name must be between 3 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenDescriptionIsTooLong()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                new string('x', Invitation.DescriptionMaxLength + 1),
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Description cannot be more than {Invitation.DescriptionMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenStartDateIsAfterEndDate()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Start time must be before end time!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsTooShort()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                "t",
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsTooLong()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                new string('x', Invitation.TitleMaxLength + 1),
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_TitleIsNull()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                null,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Title must be between {Invitation.TitleMinLength} and {Invitation.TitleMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_LocationIsTooLong()
        {
            var result = _dut.Validate(new CreateInvitationCommand(
                _title,
                _description,
                new string('x', Invitation.LocationMaxLength + 1),
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                _participants,
                null,
                _commPkgScope,
                false));

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Location cannot be more than {Invitation.LocationMaxLength} characters!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenScopeIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidScope(_type, new List<string>(), _commPkgScope)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Not a valid scope! Choose either DP with mc scope or MDP with comm pkg scope"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantListIsInvalid()
        {
            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.Equals("Each participant must contain an oid!"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenParticipantListDoesNotHaveRequiredParticipants()
        {
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor and Construction Company must be invited"));
        }

        [TestMethod]
        public async Task Validate_ShouldFail_WhenRequiredParticipantsDoNotHaveLowestSortKeys()
        {
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(_participants)).Returns(false);

            var result = await _dut.ValidateAsync(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Contractor must be first and Construction Company must be second"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenParticipant_HasNegativeSortKey()
        {
            var participants = new List<ParticipantsForCommand>
                {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new InvitedFunctionalRoleForCreateCommand("FR1", null),
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForCreateCommand(new Guid(), true),
                        null,
                        1),
                    new ParticipantsForCommand(
                        Organization.External,
                        new InvitedExternalEmailForCreateCommand("jon@test.no"),
                        null,
                        null,
                        -3)
                };

            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                null,
                _commPkgScope,
                false);


            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key for participant must be a non negative number!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenFunctinalRoleIsInvalid()
        {
            var participants = new List<ParticipantsForCommand>
                {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        null,
                        new InvitedFunctionalRoleForCreateCommand("F", null),
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForCreateCommand(new Guid(), true),
                        null,
                        1),
                    new ParticipantsForCommand(
                        Organization.External,
                        new InvitedExternalEmailForCreateCommand("jon@test.no"),
                        null,
                        null,
                        -3)
                };

            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                null,
                _commPkgScope,
                false);


            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"Functional role code must be between 3 and {Participant.FunctionalRoleCodeMaxLength} characters!"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenExternalEmailIsInvalid()
        {
            var participants = new List<ParticipantsForCommand>
                {
                    new ParticipantsForCommand(
                        Organization.Contractor,
                        null,
                        new InvitedPersonForCreateCommand(new Guid(), true),
                        null,
                        0),
                    new ParticipantsForCommand(
                        Organization.ConstructionCompany,
                        null,
                        new InvitedPersonForCreateCommand(new Guid(), true),
                        null,
                        1),
                    new ParticipantsForCommand(
                        Organization.External,
                        new InvitedExternalEmailForCreateCommand("jon@test,no"),
                        null,
                        null,
                        -3)
                };

            _invitationValidatorMock.Setup(inv => inv.IsValidParticipantList(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.RequiredParticipantsMustBeInvited(participants)).Returns(true);
            _invitationValidatorMock.Setup(inv => inv.OnlyRequiredParticipantsHaveLowestSortKeys(participants)).Returns(true);
            var command = new CreateInvitationCommand(
                _title,
                _description,
                _location,
                new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc),
                _projectName,
                _type,
                participants,
                null,
                _commPkgScope,
                false);


            var result = _dut.Validate(command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith($"One, or more, of the email-addresses for the participants is invalid."));
        }
    }
}
