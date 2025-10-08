using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Participants
{
    public class ParticipantsControllerTestsBase : TestBase
    {
        protected const string FunctionalRoleCode1 = "FRC";
        protected const string FunctionalRoleCode2 = "FRC2";
        private const string Classification = "IPO";
        private const string AzureOid = "47ff6258-0906-4849-add8-aada76ee0b0d";

        private IList<ProCoSysFunctionalRole> _pcsFunctionalRoles;
        private List<ProCoSysPerson> _personsInFunctionalRole;
        private IList<ProCoSysPerson> _signerPersons;
        private IList<ProCoSysPerson> _proCoSysPersons;

        [TestInitialize]
        public void TestInitialize()
        {
            _personsInFunctionalRole = new List<ProCoSysPerson>
            {
                new ProCoSysPerson
                {
                    AzureOid = AzureOid,
                    FirstName = "FirstName",
                    LastName = "LastName",
                    Email = "Test@email.com",
                    UserName = "UserName"
                }
            };

            _pcsFunctionalRoles = new List<ProCoSysFunctionalRole>
            {
                new ProCoSysFunctionalRole
                {
                    Code = FunctionalRoleCode1,
                    Description = "Description",
                    Email = "frEmail@test.com",
                    InformationEmail = null,
                    Persons = _personsInFunctionalRole,
                    UsePersonalEmail = true
                },
                new ProCoSysFunctionalRole
                {
                    Code = FunctionalRoleCode2,
                    Description = "Description2",
                    Email = "fr2Email@test.com",
                    InformationEmail = null,
                    Persons = _personsInFunctionalRole,
                    UsePersonalEmail = false
                }
            };

            _signerPersons = new List<ProCoSysPerson>
            {
                TestFactory.Instance.GetTestUserForUserType(UserType.Signer).Profile.AsMainProCoSysPerson()
            };

            _proCoSysPersons = new List<ProCoSysPerson>
            {
                new ProCoSysPerson
                {
                    AzureOid = AzureOid,
                    FirstName = "FirstName1",
                    LastName = "LastName1",
                    Email = "Test1@email.com",
                    UserName = "UserName1"
                },
                new ProCoSysPerson
                {
                    AzureOid = Guid.NewGuid().ToString(),
                    FirstName = "FirstName2",
                    LastName = "LastName2",
                    Email = "Test2@email.com",
                    UserName = "UserName2"
                }
            };

            TestFactory.Instance
                .MainPersonApiServiceMock
                .Setup(x => x.GetPersonsAsync(
                    TestFactory.PlantWithAccess,
                    "p",
                    It.IsAny<CancellationToken>(),
                    It.IsAny<long>()))
                .Returns(Task.FromResult(_proCoSysPersons));

            TestFactory.Instance
                .MainPersonApiServiceMock
                .Setup(x => x.GetPersonsWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    "SignersSearchString",
                    "IPO",
                    It.IsAny<List<string>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(_signerPersons));

            var viewer = TestFactory.Instance.GetTestUserForUserType(UserType.Viewer).Profile;
            TestFactory.Instance
                .MainPersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    viewer.Oid,
                    "IPO",
                    It.IsAny<List<string>>(), 
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(viewer.AsMainProCoSysPerson()));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByClassificationAsync(
                    TestFactory.PlantWithAccess,
                    Classification,
                    CancellationToken.None))
                .Returns(Task.FromResult(_pcsFunctionalRoles));
        }
    }
}
