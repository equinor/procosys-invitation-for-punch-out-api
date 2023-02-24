using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Me
{
    public class MeControllerTestsBase : TestBase
    {
        private const string AzureOid = "47ff6258-0906-4849-add8-aada76ee0b0d";
        private const string FunctionalRoleCode = "FRC";
        protected const string InvitationLocation = "InvitationLocation";
        private readonly IList<string> _functionalRoleCodes = new List<string> {FunctionalRoleCode};
        private IList<ProCoSysFunctionalRole> _pcsFunctionalRoles;
        private List<ProCoSysPerson> _personsInFunctionalRole;

        protected DateTime _invitationStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        protected DateTime _invitationEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);

        protected List<CreateParticipantsDto> _participants;
        protected List<string> _mcPkgScope;
        private ProCoSysMcPkg _mcPkgDetails;

        protected TestProfile _sigurdSigner;

        [TestInitialize]
        public void TestInitialize()
        {
            _sigurdSigner = TestFactory.Instance.GetTestUserForUserType(UserType.Signer).Profile;

            _participants = new List<CreateParticipantsDto>
            {
                new CreateParticipantsDto
                {
                    Organization = Organization.Contractor,
                    Person = _sigurdSigner.AsCreatePersonDto(true),
                    SortKey = 0
                },
                new CreateParticipantsDto
                {
                    Organization = Organization.ConstructionCompany,
                    Person = _sigurdSigner.AsCreatePersonDto(true),
                    SortKey = 1
                },
                new CreateParticipantsDto
                {
                    Organization = Organization.TechnicalIntegrity,
                    Person = _sigurdSigner.AsCreatePersonDto(false),
                    SortKey = 2
                }
            };

            const string McPkgNo = "MC1";
            _mcPkgScope = new List<string> {McPkgNo};

            _mcPkgDetails = new ProCoSysMcPkg
            {
                CommPkgNo = KnownTestData.CommPkgNo,
                Description = "D1",
                Id = 1,
                McPkgNo = McPkgNo,
                System = KnownTestData.System
            };

            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> {_mcPkgDetails};

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

            var knownGeneralMeeting = new ApiGeneralMeeting
            {
                Classification = string.Empty,
                Contract = null,
                Convention = string.Empty,
                DateCreatedUtc = DateTime.MinValue,
                DateEnd = new ApiDateTimeTimeZoneModel {DateTimeUtc = _invitationEndTime},
                DateStart = new ApiDateTimeTimeZoneModel {DateTimeUtc = _invitationStartTime},
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

            TestFactory.Instance
                .MeApiServiceMock
                .Setup(x => x.GetFunctionalRoleCodesAsync(
                    TestFactory.PlantWithAccess))
                .Returns(Task.FromResult(_functionalRoleCodes));

            TestFactory.Instance
                .McPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(
                    TestFactory.PlantWithAccess,
                    TestFactory.ProjectWithAccess,
                    _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestFactory.PlantWithAccess,
                    new List<string> {FunctionalRoleCode}))
                .Returns(Task.FromResult(_pcsFunctionalRoles));

            TestFactory.Instance
                .MainPersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _sigurdSigner.Oid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_sigurdSigner.AsMainProCoSysPerson()));

            TestFactory.Instance
                .FusionMeetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));

            TestFactory.Instance
                .EmailServiceMock
                .Setup(x => x.SendEmailsAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            TestFactory.Instance
                .MeetingOptionsMock
                .Setup(x => x.CurrentValue)
                .Returns(new MeetingOptions{PcsBaseUrl = TestFactory.PlantWithAccess});
        }
    }
}
