using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands
{
    [TestClass]
    public class FunctionalRoleForCommandValidatorTests
    {
        private FunctionalRoleForCommandValidator _dut;
        private FunctionalRoleForCommand _validCommand;
        private FunctionalRoleForCommand _invalidCommand;
        private FunctionalRoleForCommand _invalidCommand2;
        private FunctionalRoleForCommand _invalidCommand3;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand = new FunctionalRoleForCommand("FR1", "fr1@test.com", false, null);
            _invalidCommand = new FunctionalRoleForCommand(null, "fr2@test.com", false, null);
            _invalidCommand2 = new FunctionalRoleForCommand("1", "fr2@test.com", false, null);
            _invalidCommand3 = new FunctionalRoleForCommand("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?", "fr2@test.com", false, null);
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
            var result = _dut.Validate(_invalidCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCodeIs1Char()
        {
            var result = _dut.Validate(_invalidCommand2);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenCodeIsMoreThanMaxLength()
        {
            var result = _dut.Validate(_invalidCommand3);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Functional role code must be between 3 and"));
        }

    }
}
