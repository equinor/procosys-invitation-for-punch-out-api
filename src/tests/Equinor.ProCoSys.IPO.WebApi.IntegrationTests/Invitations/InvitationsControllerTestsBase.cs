using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Fusion.Integration.Http.Models;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    [TestClass]
    public class InvitationsControllerTestsBase : TestBase
    {
        private const string FunctionalRoleCode = "FRC";
        protected const string InvitationLocation = "InvitationLocation";
        private const string AzureOid = "47ff6258-0906-4849-add8-aada76ee0b0d";
        protected readonly int InitialInvitationId = TestFactory.Instance.KnownTestData.InvitationIds.First();
        protected int _attachmentId;
        protected int _commentId;
        protected DateTime _invitationStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        protected DateTime _invitationEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
        
        protected List<string> _mcPkgScope;
        protected List<ParticipantsForCommand> _participants;
        protected List<ParticipantsForCommand> _participantsForSigning;
        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;
        private IList<ProCoSysFunctionalRole> _pcsFunctionalRoles;
        private List<ProCoSysPerson> _personsInFunctionalRole;

        protected readonly TestFile FileToBeUploaded = new TestFile("test file content", "file.txt");
        protected readonly TestFile FileToBeUploaded2 = new TestFile("test file 2 content", "file2.txt");
        protected PersonHelper _sigurdSigner, _connieConstructor, _conradContractor, _pernillaPlanner;

        [TestInitialize]
        public void TestInitialize()
        {
            var personParticipant = new PersonForCommand(Guid.NewGuid(), "ola@test.com", true);
            var functionalRoleParticipant = new FunctionalRoleForCommand(FunctionalRoleCode, null);
            var completerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Completer);
            _conradContractor = new PersonHelper(completerUser.Profile.Oid, "Conrad", "Contractor", "ConradUserName","conrad@contractor.com", 1, "AAAAAAAAALA=");
            var accepterUser = TestFactory.Instance.GetTestUserForUserType(UserType.Accepter);
            _connieConstructor = new PersonHelper(accepterUser.Profile.Oid, "Connie", "Constructor", "ConnieUserName", "connie@constructor.com", 2, "AAAAAAAAABA=");
            var signerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Signer);
            _sigurdSigner = new PersonHelper(signerUser.Profile.Oid, "Sigurd", "Signer", "SigurdUserName", "sigurd@signer.com", 3, "AAAAAAAAAMA=");
            var plannerUser = TestFactory.Instance.GetTestUserForUserType(UserType.Planner);
            _pernillaPlanner = new PersonHelper(plannerUser.Profile.Oid, "Pernilla", "Planner", "PernillaUserName", "pernilla@planner.com", 4, "AAAAAAAAASA=");

            _participants = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    functionalRoleParticipant,
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    personParticipant,
                    null,
                    1)
            };

            _participantsForSigning = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    _conradContractor.AsPersonForCommand(true),
                    null,
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    _connieConstructor.AsPersonForCommand(true),
                    null,
                    1),
                new ParticipantsForCommand(
                    Organization.TechnicalIntegrity,
                    null,
                    _sigurdSigner.AsPersonForCommand(false),
                    null,
                    2)
            };

            var knownGeneralMeeting = new ApiGeneralMeeting
            {
                Classification = string.Empty,
                Contract = null,
                Convention = string.Empty,
                DateCreatedUtc = DateTime.MinValue,
                DateEnd = new ApiDateTimeTimeZoneModel 
                    { DateTimeUtc = _invitationEndTime },
                DateStart = new ApiDateTimeTimeZoneModel
                    { DateTimeUtc = _invitationStartTime },
                ExternalId = null,
                Id = KnownTestData.MeetingId,
                InviteBodyHtml = string.Empty,
                IsDisabled = false,
                IsOnlineMeeting = false,
                Location = InvitationLocation,
                Organizer = new ApiPersonDetailsV1(),
                OutlookMode = string.Empty,
                Participants = new List<ApiMeetingParticipant>
                {
                    new ApiMeetingParticipant
                    {
                        Id = Guid.NewGuid(),
                        Person = new ApiPersonDetailsV1 {Id = Guid.NewGuid(), Mail = "P1@email.com"},
                        OutlookResponse = "Required"
                    },
                    new ApiMeetingParticipant
                    {
                        Id = Guid.NewGuid(),
                        Person = new ApiPersonDetailsV1 {Id = Guid.NewGuid(), Mail = "FR1@email.com"},
                        OutlookResponse = "Accepted"
                    }
                },
                Project = null,
                ResponsiblePersons = new List<ApiPersonDetailsV1>(),
                Series = null,
                Title = string.Empty
            };

            _attachmentId = TestFactory.Instance.KnownTestData.AttachmentIds.First();
            _commentId = TestFactory.Instance.KnownTestData.CommentIds.First();

            const string McPkgNo1 = "MC1";
            const string McPkgNo2 = "MC2";

            _mcPkgScope = new List<string> {McPkgNo1, McPkgNo2};

            _mcPkgDetails1 = new ProCoSysMcPkg
            {
                CommPkgNo = KnownTestData.CommPkgNo, Description = "D1", Id = 1, McPkgNo = McPkgNo1, System = KnownTestData.System
            };
            _mcPkgDetails2 = new ProCoSysMcPkg
            {
                CommPkgNo = KnownTestData.CommPkgNo, Description = "D2", Id = 2, McPkgNo = McPkgNo2, System = KnownTestData.System
            };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> {_mcPkgDetails1, _mcPkgDetails2};

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
                    Code = FunctionalRoleCode,
                    Description = "Description",
                    Email = "frEmail@test.com",
                    InformationEmail = null,
                    Persons = _personsInFunctionalRole,
                    UsePersonalEmail = true
                }
            };

            TestFactory.Instance
                .McPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(TestFactory.PlantWithAccess, TestFactory.ProjectWithAccess,
                    _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestFactory.PlantWithAccess,
                    new List<string> {FunctionalRoleCode}))
                .Returns(Task.FromResult(_pcsFunctionalRoles));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _sigurdSigner.AzureOid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_sigurdSigner.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _connieConstructor.AzureOid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_connieConstructor.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _conradContractor.AzureOid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_conradContractor.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _pernillaPlanner.AzureOid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_pernillaPlanner.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                        TestFactory.PlantWithAccess,
                        personParticipant.AzureOid.ToString(),
                        "IPO",
                        new List<string> {"CREATE", "SIGN"}))
                .Returns(Task.FromResult(new ProCoSysPerson
                {
                    AzureOid = personParticipant.AzureOid.ToString(),
                    Email = personParticipant.Email,
                    FirstName = "Ola",
                    LastName = "Nordmann",
                    UserName = "UserName"
                }));

            TestFactory.Instance
                .FusionMeetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));

            TestFactory.Instance
                .FusionMeetingClientMock
                .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));

            TestFactory.Instance
                .MeetingOptionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new MeetingOptions{PcsBaseUrl = TestFactory.PlantWithAccess});
        }
    }
}
