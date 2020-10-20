using System;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands.CreateInvitation
{
    [TestClass]
    public class PersonForCommandValidatorTests
    {
        private PersonForCommandValidator _dut;
        private PersonForCommand _validCommand;
        private PersonForCommand _invalidCommand;
        private PersonForCommand _invalidCommand2;
        private PersonForCommand _invalidCommand3;
        private PersonForCommand _invalidCommand4;
        private PersonForCommand _invalidCommand5;
        private PersonForCommand _invalidCommand6;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "Smith", "zoey@test.com", true);
            _invalidCommand = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "", "Smith", "zoey@test.com", true);
            _invalidCommand2 = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), null, "Smith", "zoey@test.com", true);
            _invalidCommand3 = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "", "zoey@test.com", true);
            _invalidCommand4 = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", null, "zoey@test.com", true);
            _invalidCommand5 = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?", "Smith", "zoey@test.com", true);
            _invalidCommand6 = new PersonForCommand(new Guid("11111111-2222-2222-2222-333333333333"), "Zoey", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?", "zoey@test.com", true);
            _dut = new PersonForCommandValidator();
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_validCommand);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenFirstNameIsEmpty()
        {
            var result = _dut.Validate(_invalidCommand);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("First name must be between 1 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenFirstNameIsNull()
        {
            var result = _dut.Validate(_invalidCommand2);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("First name must be between 1 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenFirstNameIsTooLong()
        {
            var result = _dut.Validate(_invalidCommand5);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("First name must be between 1 and "));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenLastNameIsEmpty()
        {
            var result = _dut.Validate(_invalidCommand3);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Last name must be between 1 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenLastNameIsTooLong()
        {
            var result = _dut.Validate(_invalidCommand4);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Last name must be between 1 and"));
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenLastNameIsNull()
        {
            var result = _dut.Validate(_invalidCommand6);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Last name must be between 1 and"));
        }
    }
}
