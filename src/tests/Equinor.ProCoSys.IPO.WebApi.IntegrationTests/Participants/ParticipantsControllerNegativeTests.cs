using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Participants
{
    [TestClass]
    public class ParticipantsControllerNegativeTests : ParticipantsControllerTestsBase
    {
        #region GetFunctionalRolesForIpo

        [TestMethod]
        public async Task GetFunctionalRolesForIpo_AsAnonymous_ShouldReturnUnauthorized()
            => await ParticipantsControllerTestsHelper.GetFunctionalRolesForIpoAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetFunctionalRolesForIpo_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ParticipantsControllerTestsHelper.GetFunctionalRolesForIpoAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetFunctionalRolesForIpo_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ParticipantsControllerTestsHelper.GetFunctionalRolesForIpoAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                HttpStatusCode.Forbidden);
        #endregion

        #region GetPersons
        [TestMethod]
        public async Task GetPersons_AsAnonymous_ShouldReturnUnauthorized()
            => await ParticipantsControllerTestsHelper.GetPersonsAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "p",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetPersons_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ParticipantsControllerTestsHelper.GetPersonsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "p",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetPersons_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ParticipantsControllerTestsHelper.GetPersonsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "p",
                HttpStatusCode.Forbidden);
        #endregion

        #region GetSignerPersons
        [TestMethod]
        public async Task GetSignerPersons_AsAnonymous_ShouldReturnUnauthorized()
            => await ParticipantsControllerTestsHelper.GetSignerPersonsAsync(
                UserType.Anonymous,
                TestFactory.UnknownPlant,
                "p",
                HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task GetSignerPersons_AsHacker_ShouldReturnBadRequest_WhenUnknownPlant()
            => await ParticipantsControllerTestsHelper.GetSignerPersonsAsync(
                UserType.Hacker,
                TestFactory.UnknownPlant,
                "p",
                HttpStatusCode.BadRequest,
                "is not a valid plant");

        [TestMethod]
        public async Task GetSignerPersons_AsHacker_ShouldReturnForbidden_WhenPermissionMissing()
            => await ParticipantsControllerTestsHelper.GetSignerPersonsAsync(
                UserType.Hacker,
                TestFactory.PlantWithAccess,
                "p",
                HttpStatusCode.Forbidden);
        #endregion
    }
}
