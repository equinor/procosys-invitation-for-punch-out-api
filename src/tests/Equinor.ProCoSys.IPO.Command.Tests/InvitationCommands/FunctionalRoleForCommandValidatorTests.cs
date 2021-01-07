using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands
{
    [TestClass]
    public class FunctionalRoleForCommandValidatorTests
    {
        private FunctionalRoleForCommandValidator _dut;
        private FunctionalRoleForCommand _validCommand;
        private FunctionalRoleForCommand _invalidCommandCodeNull;
        private FunctionalRoleForCommand _invalidCommandCodeTooShort;
        private FunctionalRoleForCommand _invalidCommandCodeTooLong;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand = new FunctionalRoleForCommand("FR1", null);
            _invalidCommandCodeNull = new FunctionalRoleForCommand(null, null);
            _invalidCommandCodeTooShort = new FunctionalRoleForCommand("1", null);
            _invalidCommandCodeTooLong = new FunctionalRoleForCommand(new string('x', Participant.FunctionalRoleCodeMaxLength + 1), null);
            _dut = new FunctionalRoleForCommandValidator();
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_validCommand);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCodeIsNull()
        {
            var result = _dut.Validate(_invalidCommandCodeNull);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCodeIs1Char()
        {
            var result = _dut.Validate(_invalidCommandCodeTooShort);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCodeIsMoreThanMaxLength()
        {
            var result = _dut.Validate(_invalidCommandCodeTooLong);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

    }
}
