using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter;
using Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.Command.Tests.PersonCommands.CreateSavedFilter
{
    [TestClass]
    public class CreateSavedFilterCommandValidatorTests
    {
        private CreateSavedFilterCommand _command;
        private CreateSavedFilterCommandValidator _dut;
        private Mock<ISavedFilterValidator> _savedFilterValidatorMock;

        private readonly string _title = "Title";
        private readonly string _projectName = "Project";

        [TestInitialize]
        public void Setup_OkState()
        {
            _savedFilterValidatorMock = new Mock<ISavedFilterValidator>();

            _command = new CreateSavedFilterCommand(_projectName, _title, "Criteria", false);
            _dut = new CreateSavedFilterCommandValidator(_savedFilterValidatorMock.Object);
        }

        [TestMethod]
        public void Validate_ShouldBeValid_WhenOkState()
        {
            var result = _dut.Validate(_command);

            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_ShouldFail_WhenSavedFilterWithSameTitleForPersonAlreadyExistsInProject()
        {
            _savedFilterValidatorMock.Setup(r => r.ExistsWithSameTitleForPersonInProjectAsync(_title, _projectName, default)).Returns(Task.FromResult(true));

            var result = _dut.Validate(_command);

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.IsTrue(result.Errors[0].ErrorMessage.StartsWith("A saved filter with this title already exists!"));
        }
    }
}
