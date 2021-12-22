using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands
{
    [TestClass]
    public class ParticipantsForCommandValidatorTests
    {
        private ParticipantsForCommandValidator _dut;
        private ParticipantsForCommand _validCommand;
        private ParticipantsForCommand _invalidCommandNegativeSortKey;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand =
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new FunctionalRoleForCommand("FR1", null),
                    0,
                    null);
            _invalidCommandNegativeSortKey = new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new PersonForCommand(null,"ola@test.com", true),
                    null,
                    -1,
                    null);
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
            var result = _dut.Validate(_invalidCommandNegativeSortKey);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key must be a non negative integer!"));
        }
    }
}
