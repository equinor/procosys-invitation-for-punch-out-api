using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class ParticipantsForCommandValidatorTests
    {
        private ParticipantsForCommandValidator _dut;
        private ParticipantsForCommand _validCommand;
        private ParticipantsForCommand _invalidCommand;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand =
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new FunctionalRoleForCommand("FR1", "fr@test.com", false, null),
                    0);
            _invalidCommand = new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new PersonForCommand(null, "Ola", "Nordman", "ola@test.com", true),
                    null,
                    -1);
            _dut = new ParticipantsForCommandValidator();
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_validCommand);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenSortKeyIsNegative()
        {
            var result = _dut.Validate(_invalidCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key must be a non negative integer!"));
        }
    }
}
