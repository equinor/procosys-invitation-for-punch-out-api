using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Participants
{
    [TestClass]
    public class ParticipantsControllerTests : ParticipantsControllerTestsBase
    {
        [TestMethod]
        public async Task GetFunctionalRolesForIpo_AsViewer_ShouldGetFunctionalRolesForIpo()
        {
            // Act
            var functionalRoles = await ParticipantsControllerTestsHelper.GetFunctionalRolesForIpoAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess);

            // Assert
            var functionalRole1 = functionalRoles.First();
            var functionalRole2 = functionalRoles.Last();
            Assert.AreEqual(2, functionalRoles.Count);
            Assert.AreEqual(FunctionalRoleCode1, functionalRole1.Code);
            Assert.AreEqual(FunctionalRoleCode2, functionalRole2.Code);
        }

        [TestMethod]
        public async Task GetPersons_AsViewer_ShouldGetPersons()
        {
            // Act
            var proCoSysPersons = await ParticipantsControllerTestsHelper.GetPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "p");

            // Assert
            Assert.AreEqual(2, proCoSysPersons.Count);
            var proCoSysPerson = proCoSysPersons.First();
            Assert.AreEqual("UserName1", proCoSysPerson.UserName);
            Assert.AreEqual("FirstName1", proCoSysPerson.FirstName);
            Assert.AreEqual("LastName1", proCoSysPerson.LastName);
        }

        [TestMethod]
        public async Task GetPersons_AsViewer_NoMatchingPersons_ShouldReturnEmptyList()
        {
            // Act
            var proCoSysPersons = await ParticipantsControllerTestsHelper.GetPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "searchStringWithNoMatchingPersons");

            // Assert
            Assert.AreEqual(0, proCoSysPersons.Count);
        }

        [TestMethod]
        public async Task GetSignerPersons_AsViewer_ShouldGetAdditionalSignerPersons()
        {
            // Act
            var signerPersons = await ParticipantsControllerTestsHelper.GetSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "SignersSearchString");

            // Assert
            Assert.AreEqual(1, signerPersons.Count);
            var additionalSignerPerson = signerPersons.First();
            Assert.AreEqual("SigurdUserName", additionalSignerPerson.UserName);
            Assert.AreEqual("Sigurd", additionalSignerPerson.FirstName);
            Assert.AreEqual("Signer", additionalSignerPerson.LastName);
        }

        [TestMethod]
        public async Task GetSignerPersons_AsViewer_NoMatchingPersons_ShouldReturnEmptyList()
        {
            // Act
            var proCoSysPersons = await ParticipantsControllerTestsHelper.GetSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "searchStringWithNoMatchingPersons");

            // Assert
            Assert.AreEqual(0, proCoSysPersons.Count);
        }
    }
}
