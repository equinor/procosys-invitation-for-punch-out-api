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
        public async Task GetRequiredSignerPersons_AsViewer_ShouldGetRequiredSignerPersons()
        {
            // Act
            var signerPersons = await ParticipantsControllerTestsHelper.GetRequiredSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "RequiredSignersSearchString");

            // Assert
            Assert.AreEqual(2, signerPersons.Count);
            var requiredSignerPerson = signerPersons.First();
            Assert.AreEqual("ConnieUserName", requiredSignerPerson.UserName);
            Assert.AreEqual("Connie", requiredSignerPerson.FirstName);
            Assert.AreEqual("Constructor", requiredSignerPerson.LastName);
        }

        [TestMethod]
        public async Task GetRequiredSignerPersons_AsViewer_NoMatchingPersons_ShouldReturnEmptyList()
        {
            // Act
            var proCoSysPersons = await ParticipantsControllerTestsHelper.GetRequiredSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "searchStringWithNoMatchingPersons");

            // Assert
            Assert.AreEqual(0, proCoSysPersons.Count);
        }

        [TestMethod]
        public async Task GetAdditionalSignerPersons_AsViewer_ShouldGetAdditionalSignerPersons()
        {
            // Act
            var additionalSignerPersons = await ParticipantsControllerTestsHelper.GetAdditionalSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "AdditionalSignersSearchString");

            // Assert
            Assert.AreEqual(1, additionalSignerPersons.Count);
            var additionalSignerPerson = additionalSignerPersons.First();
            Assert.AreEqual("SigurdUserName", additionalSignerPerson.UserName);
            Assert.AreEqual("Sigurd", additionalSignerPerson.FirstName);
            Assert.AreEqual("Signer", additionalSignerPerson.LastName);
        }

        [TestMethod]
        public async Task GetAdditionalSignerPersons_AsViewer_NoMatchingPersons_ShouldReturnEmptyList()
        {
            // Act
            var proCoSysPersons = await ParticipantsControllerTestsHelper.GetAdditionalSignerPersonsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                "searchStringWithNoMatchingPersons");

            // Assert
            Assert.AreEqual(0, proCoSysPersons.Count);
        }
    }
}
