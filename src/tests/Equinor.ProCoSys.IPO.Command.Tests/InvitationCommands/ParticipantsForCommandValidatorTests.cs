using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.Command.Tests.InvitationCommands
{
    [TestClass]
    public class ParticipantsForCommandValidatorTests
    {
        private ParticipantsForCommandValidator _dut;
        private EditParticipantsForCommand _validCommand;
        private EditParticipantsForCommand _invalidCommandNegativeSortKey;

        [TestInitialize]
        public void Setup_OkState()
        {

            _validCommand = new EditParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new EditFunctionalRoleForCommand("FR1", null),
                    0);
            _invalidCommandNegativeSortKey = new EditParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new EditPersonForCommand(null,"ola@test.com", true),
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
            var result = _dut.Validate(_invalidCommandNegativeSortKey);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("Sort key must be a non negative integer!"));
        }
    }
}
