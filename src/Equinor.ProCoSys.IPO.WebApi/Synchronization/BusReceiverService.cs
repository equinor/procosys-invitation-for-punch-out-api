using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Text.Json;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.PcsBus;
using Equinor.ProCoSys.PcsBus.Receiver.Interfaces;
using Equinor.ProCoSys.PcsBus.Topics;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;
using Fusion.Integration.Meeting;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public class BusReceiverService : IBusReceiverService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPlantSetter _plantSetter;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IReadOnlyContext _context;
        private readonly IFusionMeetingClient _meetingClient;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly IApplicationAuthenticator _authenticator;
        private readonly IBearerTokenSetter _bearerTokenSetter;
        private const string IpoBusReceiverTelemetryEvent = "IPO Bus Receiver";

        public BusReceiverService(
            IInvitationRepository invitationRepository,
            IPlantSetter plantSetter,
            IUnitOfWork unitOfWork,
            ITelemetryClient telemetryClient,
            IReadOnlyContext context,
            IFusionMeetingClient meetingClient,
            IMcPkgApiService mcPkgApiService,
            IApplicationAuthenticator authenticator,
            IBearerTokenSetter bearerTokenSetter)
        {
            _invitationRepository = invitationRepository;
            _plantSetter = plantSetter;
            _unitOfWork = unitOfWork;
            _telemetryClient = telemetryClient;
            _context = context;
            _meetingClient = meetingClient;
            _mcPkgApiService = mcPkgApiService;
            _authenticator = authenticator;
            _bearerTokenSetter = bearerTokenSetter;
        }

        public async Task ProcessMessageAsync(PcsTopic pcsTopic, Message message, CancellationToken token)
        {
            var messageJson = Encoding.UTF8.GetString(message.Body);

            switch (pcsTopic)
            {
                case PcsTopic.Ipo:
                    await ProcessIpoEvent(messageJson);
                    break;
                case PcsTopic.Project:
                    ProcessProjectEvent(messageJson);
                    break;
                case PcsTopic.CommPkg:
                    ProcessCommPkgEvent(messageJson);
                    break;
                case PcsTopic.McPkg:
                    ProcessMcPkgEvent(messageJson);
                    break;
            }
            await _unitOfWork.SaveChangesAsync(token);
        }

        private void ProcessMcPkgEvent(string messageJson)
        {
            var mcPkgEvent = JsonSerializer.Deserialize<McPkgTopic>(messageJson);
            if (string.IsNullOrWhiteSpace(mcPkgEvent.ProjectSchema) || string.IsNullOrWhiteSpace(mcPkgEvent.CommPkgNo) || string.IsNullOrWhiteSpace(mcPkgEvent.McPkgNo))
            {
                throw new Exception($"Unable to deserialize JSON to McPkgEvent {messageJson}");
            }

            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {BusReceiverTelemetryConstants.Event, IpoTopic.TopicName},
                    {BusReceiverTelemetryConstants.McPkgNo, mcPkgEvent.McPkgNo},
                    {BusReceiverTelemetryConstants.ProjectSchema, mcPkgEvent.ProjectSchema[4..]},
                    {BusReceiverTelemetryConstants.ProjectName, mcPkgEvent.ProjectName.Replace('$', '_')}
                });
            _plantSetter.SetPlant(mcPkgEvent.ProjectSchema);
            _invitationRepository.UpdateMcPkgOnInvitations(mcPkgEvent.ProjectName, mcPkgEvent.McPkgNo, mcPkgEvent.Description);
        }

        private void ProcessCommPkgEvent(string messageJson)
        {
            var commPkgEvent = JsonSerializer.Deserialize<CommPkgTopic>(messageJson);
            if (string.IsNullOrWhiteSpace(commPkgEvent.ProjectSchema)  || string.IsNullOrWhiteSpace(commPkgEvent.CommPkgNo))
            {
                throw new Exception($"Unable to deserialize JSON to CommPkgEvent {messageJson}");
            }

            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {BusReceiverTelemetryConstants.Event, IpoTopic.TopicName},
                    {BusReceiverTelemetryConstants.CommPkgNo, commPkgEvent.CommPkgNo},
                    {BusReceiverTelemetryConstants.ProjectSchema, commPkgEvent.ProjectSchema[4..]},
                    {BusReceiverTelemetryConstants.ProjectName, commPkgEvent.ProjectName.Replace('$', '_')}
                });
            _plantSetter.SetPlant(commPkgEvent.ProjectSchema);
            _invitationRepository.UpdateCommPkgOnInvitations(commPkgEvent.ProjectName, commPkgEvent.CommPkgNo,
                commPkgEvent.Description);
        }

        private void ProcessProjectEvent(string messageJson)
        {
            var projectEvent = JsonSerializer.Deserialize<ProjectTopic>(messageJson);
            if (string.IsNullOrWhiteSpace(projectEvent.ProjectSchema) || string.IsNullOrWhiteSpace(projectEvent.ProjectName))
            {
                throw new Exception($"Unable to deserialize JSON to ProjectEvent {messageJson}");
            }

            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {BusReceiverTelemetryConstants.Event, IpoTopic.TopicName},
                    {BusReceiverTelemetryConstants.ProjectSchema, projectEvent.ProjectSchema[4..]},
                    {BusReceiverTelemetryConstants.ProjectName, projectEvent.ProjectName.Replace('$', '_')}
                });
            _plantSetter.SetPlant(projectEvent.ProjectSchema);
            _invitationRepository.UpdateProjectOnInvitations(projectEvent.ProjectName, projectEvent.Description);
        }

        private async Task ProcessIpoEvent(string messageJson)
        {
            var bearerToken = await _authenticator.GetBearerTokenForApplicationAsync();
            _bearerTokenSetter.SetBearerToken(bearerToken, false);

            var ipoEvent = JsonSerializer.Deserialize<IpoTopic>(messageJson);
            if (string.IsNullOrWhiteSpace(ipoEvent.InvitationGuid))
            {
                throw new Exception($"Unable to deserialize JSON to IpoEvent {messageJson}");
            }

            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {BusReceiverTelemetryConstants.Event, IpoTopic.TopicName},
                    {BusReceiverTelemetryConstants.ProjectSchema, ipoEvent.ProjectSchema[4..]},
                    {BusReceiverTelemetryConstants.Ipo, ipoEvent.InvitationGuid},
                    {BusReceiverTelemetryConstants.IpoEvent, ipoEvent.Event}
                });
            _plantSetter.SetPlant(ipoEvent.ProjectSchema);
            var invitation = _context.QuerySet<Invitation>().Include(i => i.McPkgs).Include(i => i.CommPkgs)
                .SingleOrDefault(i => i.ObjectGuid == Guid.Parse(ipoEvent.InvitationGuid));
            if (invitation == null)
            {
                throw new Exception($"Invitation {ipoEvent.InvitationGuid} not found");
            }

            if (ipoEvent.Event == "Canceled")
            {
                // For a cancel we also clear the external reference.
                // TODO: We have to remove this confusing logic around sending/not sending invitationId based on what should happen in main...
                await ClearM01DatesAndKeepExternalReferenceAndCancelMeeting(ipoEvent, invitation);
            }
            else if (ipoEvent.Event == "Completed")
            {
                await SetM01Dates(ipoEvent, invitation);
            }
            else if (ipoEvent.Event == "Accepted")
            {
                await SetM02DatesAsync(ipoEvent, invitation);
            }
            else if (ipoEvent.Event == "UnAccepted")
            {
                await ClearM02DateAsync(ipoEvent, invitation);
            }
            else if (ipoEvent.Event == "UnCompleted")
            {
                // For a completed IPO we leave the external reference.
                // TODO: We have to remove this confusing logic around sending/not sending invitationId based on what should happen in main...
                await ClearM01DatesAndBlankExternalReferenceAsync(ipoEvent, invitation);
            }
        }

        private async Task ClearM01DatesAndBlankExternalReferenceAsync(IpoTopic ipoEvent, Invitation invitation)
        {
            try
            {
                await _mcPkgApiService.ClearM01DatesAsync(
                    ipoEvent.ProjectSchema,
                    null,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception e)
            {
                throw new Exception("Error: Could not clear M-01 dates", e);
            }
        }

        private async Task SetM01Dates(IpoTopic ipoEvent, Invitation invitation)
        {
            try
            {
                await _mcPkgApiService.SetM01DatesAsync(
                    ipoEvent.ProjectSchema,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception e)
            {
                throw new Exception($"Error: Could not set M-01 dates for {invitation.ObjectGuid}", e);
            }
        }

        private async Task ClearM01DatesAndKeepExternalReferenceAndCancelMeeting(IpoTopic ipoEvent, Invitation invitation)
        {
            if (ipoEvent.Status == (int) IpoStatus.Completed)
            {
                try
                {
                    await _mcPkgApiService.ClearM01DatesAsync(
                        ipoEvent.ProjectSchema,
                        invitation.Id,
                        invitation.ProjectName,
                        invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                        invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
                }
                catch (Exception e)
                {
                    throw new Exception($"Error: Could not clear M-01 dates for {invitation.ObjectGuid}", e);
                }
            }

            try
            {
                await _meetingClient.DeleteMeetingAsync(invitation.MeetingId);
            }
            catch (Exception e)
            {
                throw new Exception($"Error: Could not cancel outlook meeting {invitation.MeetingId}.", e);
            }
        }

        private async Task SetM02DatesAsync(IpoTopic ipoEvent, Invitation invitation)
        {
            try
            {
                await _mcPkgApiService.SetM02DatesAsync(
                    ipoEvent.ProjectSchema,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception e)
            {
                throw new Exception("Error: Could not set M-02 dates", e);
            }
        }

        private async Task ClearM02DateAsync(IpoTopic ipoEvent, Invitation invitation)
        {
            try
            {
                await _mcPkgApiService.ClearM02DatesAsync(
                    ipoEvent.ProjectSchema,
                    invitation.Id,
                    invitation.ProjectName,
                    invitation.McPkgs.Select(mcPkg => mcPkg.McPkgNo).ToList(),
                    invitation.CommPkgs.Select(c => c.CommPkgNo).ToList());
            }
            catch (Exception e)
            {
                throw new Exception("Error: Could not clear M-02 dates", e);
            }
        }
    }
}
