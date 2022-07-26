using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.CreateInvitation;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.EditInvitation;
using Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation;
using Fusion.Integration.Http.Models;
using Fusion.Integration.Meeting;
using Fusion.Integration.Meeting.Http.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations
{
    public class InvitationsControllerTestsBase : TestBase
    {
        protected const string FunctionalRoleCode = "FRCA";
        protected const string InvitationLocation = "InvitationLocation";
        protected readonly int InitialMdpInvitationId = TestFactory.Instance.KnownTestData.MdpInvitationIds.First();
        protected readonly int InitialDpInvitationId = TestFactory.Instance.KnownTestData.DpInvitationIds.First();
        protected int _commentId;
        protected DateTime _invitationStartTime = new DateTime(2020, 9, 1, 12, 0, 0, DateTimeKind.Utc);
        protected DateTime _invitationEndTime = new DateTime(2020, 9, 1, 13, 0, 0, DateTimeKind.Utc);
        protected AttachmentDto _attachmentOnInitialMdpInvitation;

        protected List<string> _mcPkgScope;
        protected List<CreateParticipantsDto> _participants;
        protected List<CreateParticipantsDto> _participantsForSigning;
        private ProCoSysMcPkg _mcPkgDetails1;
        private ProCoSysMcPkg _mcPkgDetails2;

        protected TestProfile _sigurdSigner;
        protected TestProfile _contractor;
        protected TestProfile _pernillaPlanner;
        protected TestProfile _andreaAdmin;

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            _sigurdSigner = TestFactory.Instance.GetTestUserForUserType(UserType.Signer).Profile;
            _pernillaPlanner = TestFactory.Instance.GetTestUserForUserType(UserType.Planner).Profile;
            _contractor = TestFactory.Instance.GetTestUserForUserType(UserType.Contractor).Profile;
            _andreaAdmin = TestFactory.Instance.GetTestUserForUserType(UserType.Admin).Profile;

            var personParticipant = new CreateInvitedPersonDto
            {
                AzureOid = Guid.NewGuid(),
                Email = "ola@test.com",
                Required = true
            };
            var person1InFunctionalRoleParticipant = new CreateInvitedPersonDto
            {
                AzureOid = new Guid(_contractor.Oid),
                Email = "per@test.com"
            };
            var person2InFunctionalRoleParticipant = new CreateInvitedPersonDto
            {
                AzureOid = Guid.NewGuid(),
                Email = "pål@test.com"
            };
            var functionalRoleParticipant = new CreateFunctionalRoleDto
            {
                Code = FunctionalRoleCode,
                Persons = new List<CreateInvitedPersonDto>
                {
                    person1InFunctionalRoleParticipant,
                    person2InFunctionalRoleParticipant
                }
            };

            _participants = new List<CreateParticipantsDto>
            {
                new CreateParticipantsDto
                {
                    Organization = Organization.Contractor,
                    FunctionalRole = functionalRoleParticipant,
                    SortKey = 0
                },
                new CreateParticipantsDto
                {
                    Organization = Organization.ConstructionCompany,
                    Person = personParticipant,
                    SortKey = 1
                }
            };

            _participantsForSigning = new List<CreateParticipantsDto>
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

            TestFactory.Instance
                .McPkgApiServiceMock
                .Setup(x => x.GetMcPkgsByMcPkgNosAsync(TestFactory.PlantWithAccess, TestFactory.ProjectWithAccess,
                    _mcPkgScope))
                .Returns(Task.FromResult(mcPkgDetails));

            var personsInFunctionalRole = new List<ProCoSysPerson>
            {
                new ProCoSysPerson
                {
                    AzureOid = person1InFunctionalRoleParticipant.AzureOid.ToString(),
                    FirstName = "Per",
                    LastName = "Persen",
                    Email = person1InFunctionalRoleParticipant.Email,
                    UserName = "User1"
                },
                new ProCoSysPerson
                {
                    AzureOid = person2InFunctionalRoleParticipant.AzureOid.ToString(),
                    FirstName = "Pål",
                    LastName = "Persen",
                    Email = person2InFunctionalRoleParticipant.Email,
                    UserName = "User2"
                }
            };

            IList<ProCoSysFunctionalRole> pcsFunctionalRoles1 = new List<ProCoSysFunctionalRole>
            {
                new ProCoSysFunctionalRole
                {
                    Code = KnownTestData.FunctionalRoleCode,
                    Description = "Description",
                    Email = "frEmail@test.com",
                    InformationEmail = null,
                    Persons = personsInFunctionalRole,
                    UsePersonalEmail = true
                }
            };

            IList<ProCoSysFunctionalRole> pcsFunctionalRoles2 = new List<ProCoSysFunctionalRole>
            {
                new ProCoSysFunctionalRole
                {
                    Code = FunctionalRoleCode,
                    Description = "Description",
                    Email = "frEmail@test.com",
                    InformationEmail = null,
                    Persons = personsInFunctionalRole,
                    UsePersonalEmail = true
                }
            };

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestFactory.PlantWithAccess,
                    new List<string> { KnownTestData.FunctionalRoleCode }))
                .Returns(Task.FromResult(pcsFunctionalRoles1));

            TestFactory.Instance
                .FunctionalRoleApiServiceMock
                .Setup(x => x.GetFunctionalRolesByCodeAsync(TestFactory.PlantWithAccess,
                    new List<string> { FunctionalRoleCode }))
                .Returns(Task.FromResult(pcsFunctionalRoles2));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _sigurdSigner.Oid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_sigurdSigner.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _contractor.Oid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_contractor.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _pernillaPlanner.Oid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_pernillaPlanner.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                    TestFactory.PlantWithAccess,
                    _andreaAdmin.Oid,
                    "IPO",
                    It.IsAny<List<string>>()))
                .Returns(Task.FromResult(_andreaAdmin.AsProCoSysPerson()));

            TestFactory.Instance
                .PersonApiServiceMock
                .Setup(x => x.GetPersonByOidWithPrivilegesAsync(
                        TestFactory.PlantWithAccess,
                        personParticipant.AzureOid.ToString(),
                        "IPO",
                        new List<string> {"SIGN"}))
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
            _attachmentOnInitialMdpInvitation = await UploadAttachmentAsync(InitialMdpInvitationId);
        }

        internal async Task<(int, UnAcceptPunchOutDto)> CreateValidUnAcceptPunchOutDtoAsync(List<CreateParticipantsDto> participants)
        {
            var (invitationToAcceptId, acceptPunchOutDto) = await CreateValidAcceptPunchOutDtoAsync(participants);

            await InvitationsControllerTestsHelper.AcceptPunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId,
                acceptPunchOutDto);

            var acceptedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToAcceptId);

            var accepterParticipant = acceptedInvitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var unAcceptPunchOutDto = new UnAcceptPunchOutDto
            {
                InvitationRowVersion = acceptedInvitation.RowVersion,
                ParticipantRowVersion = accepterParticipant.RowVersion,
            };

            return (invitationToAcceptId, unAcceptPunchOutDto);
        }

        internal async Task<(int, ParticipantToChangeDto[])> CreateValidParticipantToChangeDtosAsync(List<CreateParticipantsDto> participants)
        {
            var (invitationToChangeId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(participants);

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId,
                completePunchOutDto);

            var completedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToChangeId);

            var completerParticipant = completedInvitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var participantToChangeDtos = new[]
            {
                new ParticipantToChangeDto
                {
                    Id = completerParticipant.Id,
                    Attended = false,
                    Note = $"Some note about the punch round or attendee {Guid.NewGuid():B}",
                    RowVersion = completerParticipant.RowVersion
                }
            };

            return (invitationToChangeId, participantToChangeDtos);
        }

        internal async Task<(int, AcceptPunchOutDto)> CreateValidAcceptPunchOutDtoAsync(List<CreateParticipantsDto> participants)
        {
            var (invitationToCompleteAndAcceptId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(participants);

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteAndAcceptId,
                completePunchOutDto);

            var completedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteAndAcceptId);

            var accepterParticipant = completedInvitation.Participants
                .Single(p => p.Organization == Organization.ConstructionCompany);

            var acceptPunchOutDto = new AcceptPunchOutDto
            {
                InvitationRowVersion = completedInvitation.RowVersion,
                ParticipantRowVersion = accepterParticipant.RowVersion,
                Participants = new List<ParticipantToUpdateNoteDto>
                {
                    new ParticipantToUpdateNoteDto
                    {
                        Id = accepterParticipant.Id,
                        Note = $"Some note about the punch round or attendee {Guid.NewGuid():B}",
                        RowVersion = accepterParticipant.RowVersion
                    }
                }
            };

            return (invitationToCompleteAndAcceptId, acceptPunchOutDto);
        }

        internal async Task<(int, UnCompletePunchOutDto)> CreateValidUnCompletePunchOutDtoAsync(List<CreateParticipantsDto> participants)
        {
            var (invitationToCompleteAndUnCompleteId, completePunchOutDto) = await CreateValidCompletePunchOutDtoAsync(participants);

            await InvitationsControllerTestsHelper.CompletePunchOutAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteAndUnCompleteId,
                completePunchOutDto);

            var completedInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Signer,
                TestFactory.PlantWithAccess,
                invitationToCompleteAndUnCompleteId);

            var completerParticipant = completedInvitation.Participants
                .Single(p => p.Organization == Organization.Contractor);
            var unCompletePunchOutDto = new UnCompletePunchOutDto
            {
                InvitationRowVersion = completedInvitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
            };

            return (invitationToCompleteAndUnCompleteId, unCompletePunchOutDto);
        }

        internal async Task<(int, string)> CreateValidDeletePunchOutDtoAsync(List<CreateParticipantsDto> participants, UserType userType = UserType.Planner)
        {
            var (invitationId, cancelPunchOutDto) = await CreateValidCancelPunchOutDtoAsync(participants, userType);

            await InvitationsControllerTestsHelper.CancelPunchOutAsync(
                UserType.Admin,
                TestFactory.PlantWithAccess,
                invitationId,
                cancelPunchOutDto);

            var canceledInvitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Admin,
                TestFactory.PlantWithAccess,
                invitationId);

            return (invitationId, canceledInvitation.RowVersion);
        }

        internal async Task<(int, CompletePunchOutDto)> CreateValidCompletePunchOutDtoAsync(List<CreateParticipantsDto> participants)
        {
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                participants,
                _mcPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            var completerParticipant = invitation.Participants
                .Single(p => p.Organization == Organization.Contractor);

            var completePunchOutDto = new CompletePunchOutDto
            {
                InvitationRowVersion = invitation.RowVersion,
                ParticipantRowVersion = completerParticipant.RowVersion,
                Participants = new List<ParticipantToChangeDto>
                {
                    new ParticipantToChangeDto
                    {
                        Id = completerParticipant.Id,
                        Note = $"Some note about the punch round or attendee {Guid.NewGuid():B}",
                        RowVersion = completerParticipant.RowVersion,
                        Attended = true
                    }
                }
            };

            return (id, completePunchOutDto);
        }

        internal async Task<(int, EditInvitedInvitationDto)> CreateValidEditInvitationDtoAsync(IList<CreateParticipantsDto> participants)
        {
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                participants,
                _mcPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            var editInvitationDto = new EditInvitedInvitationDto
            {
                Title = invitation.Title,
                Description = invitation.Description,
                StartTime = invitation.StartTimeUtc,
                EndTime = invitation.EndTimeUtc,
                Location = invitation.Location,
                ProjectName = invitation.ProjectName,
                RowVersion = invitation.RowVersion,
                UpdatedParticipants = ConvertToParticipantDtoEdit(invitation.Participants),
                UpdatedCommPkgScope = null,
                UpdatedMcPkgScope = _mcPkgScope
            };

            return (id, editInvitationDto);
        }

        internal async Task<AttachmentDto> UploadAttachmentAsync(int invitationId)
        {
            var fileToBeUploaded = TestFile.NewFileToBeUploaded();
            await InvitationsControllerTestsHelper.UploadAttachmentAsync(
                UserType.Planner,
                TestFactory.PlantWithAccess,
                invitationId,
                fileToBeUploaded);

            var attachmentDtos = await InvitationsControllerTestsHelper.GetAttachmentsAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                invitationId);
            
            return attachmentDtos.Single(t => t.FileName == fileToBeUploaded.FileName);
        }

        internal async Task<(int, CancelPunchOutDto)> CreateValidCancelPunchOutDtoAsync(List<CreateParticipantsDto> participants, UserType userType = UserType.Planner)
        {
            var id = await InvitationsControllerTestsHelper.CreateInvitationAsync(
                userType,
                TestFactory.PlantWithAccess,
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                InvitationLocation,
                DisciplineType.DP,
                _invitationStartTime,
                _invitationEndTime,
                participants,
                _mcPkgScope,
                null);

            var invitation = await InvitationsControllerTestsHelper.GetInvitationAsync(
                UserType.Viewer,
                TestFactory.PlantWithAccess,
                id);

            var cancelPunchOutDto = new CancelPunchOutDto
            {
                RowVersion = invitation.RowVersion
            };

            return (id, cancelPunchOutDto);
        }

        private IEnumerable<EditParticipantsDto> ConvertToParticipantDtoEdit(IEnumerable<ParticipantDto> participants)
        {
            var editVersionParticipantDtos = new List<EditParticipantsDto>();
            participants.ToList().ForEach(p => editVersionParticipantDtos.Add(
                new EditParticipantsDto
                {
                    ExternalEmail = p.ExternalEmail != null ? new EditExternalEmailDto
                    {
                        Id = p.Id,
                        Email = p.ExternalEmail.ExternalEmail,
                        RowVersion = p.RowVersion
                    } : null,
                    FunctionalRole = p.FunctionalRole != null ? new EditFunctionalRoleDto
                    {
                        Id = p.Id,
                        Code = p.FunctionalRole.Code,
                        RowVersion = p.RowVersion,
                        Persons = p.FunctionalRole.Persons?.Select(person =>
                                new EditInvitedPersonDto
                                {
                                    AzureOid = person.AzureOid,
                                    Email = person.Email,
                                    Id = person.Id,
                                    Required = person.Required, 
                                    RowVersion = person.RowVersion
                                }).ToList()
                    } : null,
                    Organization = p.Organization,
                    Person = p.Person != null ? new EditInvitedPersonDto
                    {
                        Id = p.Id,
                        AzureOid = p.Person.AzureOid,
                        Email = p.Person.Email,
                        RowVersion = p.RowVersion
                    } : null,
                    SortKey = p.SortKey
                }));

            return editVersionParticipantDtos;
        }
    }
}
