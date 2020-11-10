using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        protected int InitialInvitationId = TestFactory.SeedingData.InvitationIds.First();

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
                Id = SeedingData.MeetingId,
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

            TestFactory
                .FusionMeetingClientMock
                    .Setup(x => x.GetMeetingAsync(It.IsAny<Guid>(), It.IsAny<Action<ODataQuery>>()))
                    .Returns(Task.FromResult(new GeneralMeeting(knownGeneralMeeting)));
        }
    }
}
