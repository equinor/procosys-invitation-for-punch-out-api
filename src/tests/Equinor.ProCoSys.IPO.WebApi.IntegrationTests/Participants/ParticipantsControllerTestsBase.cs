using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations;
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
       
        protected PersonHelper _sigurdSigner, _connieConstructor, _conradContractor, _vidarViewer;

        private IList<ProCoSysFunctionalRole> _pcsFunctionalRoles;
        private List<ProCoSysPerson> _personsInFunctionalRole;
        private IList<ProCoSysPerson> _requiredSignerPersons;
        private IList<ProCoSysPerson> _additionalSignerPersons;
        private IList<ProCoSysPerson> _proCoSysPersons;

        [TestInitialize]
        public void TestInitialize()
        {
            var completerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Completer);
            _conradContractor = new PersonHelper(completerUser.Profile.Oid, "Conrad", "Contractor", "ConradUserName",
                "conrad@contractor.com", 1, "AAAAAAAAALA=");
            var accepterUser = TestFactory.Instance.GetTestUserForUserType(UserType.Accepter);
            _connieConstructor = new PersonHelper(accepterUser.Profile.Oid, "Connie", "Constructor", "ConnieUserName",
                "connie@constructor.com", 2, "AAAAAAAAABA=");
            var signerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Signer);
            _sigurdSigner = new PersonHelper(signerUser.Profile.Oid, "Sigurd", "Signer", "SigurdUserName",
                "sigurd@signer.com", 3, "AAAAAAAAAMA=");
            var viewerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Viewer);
            _vidarViewer = new PersonHelper(viewerUser.Profile.Oid, "Vidar", "Viewer", "VidarUserName",
                "vidar@viewer.com", 5, "AAAAAAAAASA=");

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

            _requiredSignerPersons = new List<ProCoSysPerson>
            {
                _connieConstructor.AsProCoSysPerson(),
                _conradContractor.AsProCoSysPerson()
            };

            _additionalSignerPersons = new List<ProCoSysPerson>
            {
                _sigurdSigner.AsProCoSysPerson()
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
                .PersonApiServiceMock
                .Setup(x => x.GetPersonsAsync(
                    TestFactory.PlantWithAccess,
                    "p",
                    It.IsAny<long>()))
                .Returns(Task.FromResult(_proCoSysPersons));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonsWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    "RequiredSignersSearchString",
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_requiredSignerPersons));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonsWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    "AdditionalSignersSearchString",
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_additionalSignerPersons));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _vidarViewer.AzureOid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_vidarViewer.AsProCoSysPerson()));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByClassificationAsync(
                    TestFactory.PlantWithAccess,
                    Classification))
                .Returns(Task.FromResult(_pcsFunctionalRoles));
        }
    }
}
