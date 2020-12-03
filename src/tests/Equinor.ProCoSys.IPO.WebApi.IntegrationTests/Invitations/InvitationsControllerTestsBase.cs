using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
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
        protected int InitialInvitationId = TestFactory.Instance.KnownTestData.InvitationIds.First();
        protected int AttachmentId;
        //protected int CommpkgId;
        protected string FunctionalRoleCode = "FRC";
        protected string AzureOid = "47ff6258-0906-4849-add8-aada76ee0b0d";
        protected List<string> McPkgScope;
        //protected List<string> CommPkgScope;
        protected List<ParticipantsForCommand> Participants;
        protected ProCoSysMcPkg _mcPkgDetails1;
        protected ProCoSysMcPkg _mcPkgDetails2;
        protected IList<ProCoSysFunctionalRole> _pcsFunctionalRoles;
        private List<Person> _personsInFunctionalRole;

        protected readonly TestFile FileToBeUploaded = new TestFile("test file content", "file.txt");

        [TestInitialize]
        public void TestInitialize()
        {
            var knownGeneralMeeting = new ApiGeneralMeeting
            {
                Classification = string.Empty,
                Contract = null,
                Convention = string.Empty,
                DateCreatedUtc = DateTime.MinValue,
                DateEnd = new ApiDateTimeTimeZoneModel(),
                DateStart = new ApiDateTimeTimeZoneModel(),
                ExternalId = null,
                Id = KnownTestData.MeetingId,
                InviteBodyHtml = string.Empty,
                IsDisabled = false,
                IsOnlineMeeting = false,
                Location = string.Empty,
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

            AttachmentId = TestFactory.Instance.KnownTestData.AttachmentIds.First();
            //CommpkgId = TestFactory.Instance.KnownTestData.CommPkgIds.First();

            var _mcPkgNo1 = "MC1";
            var _mcPkgNo2 = "MC2";

            McPkgScope = new List<string> {_mcPkgNo1, _mcPkgNo2};

            TestFactory.Instance
                .FusionMeetingClientMock
                .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));

            Participants = new List<ParticipantsForCommand>
            {
                new ParticipantsForCommand(
                    Organization.Contractor,
                    null,
                    null,
                    new FunctionalRoleForCommand(FunctionalRoleCode, null),
                    0),
                new ParticipantsForCommand(
                    Organization.ConstructionCompany,
                    null,
                    new PersonForCommand(Guid.NewGuid(), "Ola", "Nordman", "ola@test.com", true),
                    null,
                    1)
            };

            _mcPkgDetails1 = new ProCoSysMcPkg
            {
                CommPkgNo = KnownTestData.CommPkgNo, Description = "D1", Id = 1, McPkgNo = _mcPkgNo1
            };
            _mcPkgDetails2 = new ProCoSysMcPkg
            {
                CommPkgNo = KnownTestData.CommPkgNo, Description = "D2", Id = 2, McPkgNo = _mcPkgNo2
            };
            IList<ProCoSysMcPkg> mcPkgDetails = new List<ProCoSysMcPkg> {_mcPkgDetails1, _mcPkgDetails2};

            _personsInFunctionalRole = new List<Person>
            {
                new Person
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

            var personInFunctionalRole = _personsInFunctionalRole.First();

            TestFactory.Instance
                .McPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(TestFactory.PlantWithAccess, TestFactory.ProjectWithAccess,
                    McPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestFactory.PlantWithAccess,
                    new List<string> {FunctionalRoleCode}))
                .Returns(Task.FromResult(_pcsFunctionalRoles));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidsInUserGroupAsync(TestFactory.PlantWithAccess, It.IsAny<string>(),
                    "MC_LEAD_DISCIPLINE"))
                .Returns(Task.FromResult(new ProCoSysPerson
                {
                    AzureOid = AzureOid,
                    Email = personInFunctionalRole.Email,
                    FirstName = personInFunctionalRole.FirstName,
                    LastName = personInFunctionalRole.LastName,
                    UserName = personInFunctionalRole.UserName
                }));

            TestFactory.Instance
                .FusionMeetingClientMock
                .Setup(x => x.CreateMeetingAsync(It.IsAny<Action<GeneralMeetingBuilder>>()))
                .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));
        }
    }
}
