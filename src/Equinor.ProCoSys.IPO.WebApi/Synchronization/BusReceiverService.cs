using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using System.Text.Json;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.BusReceiver;
using Equinor.ProCoSys.BusReceiver.Interfaces;
using Equinor.ProCoSys.BusReceiver.Topics;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public class BusReceiverService : IBusReceiverService
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPlantSetter _plantSetter;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITelemetryClient _telemetryClient;
        private const string IpoBusReceiverTelemetryEvent = "IPO Bus Receiver";

        public BusReceiverService(IInvitationRepository invitationRepository, IPlantSetter plantSetter, IUnitOfWork unitOfWork, ITelemetryClient telemetryClient)
        {
            _invitationRepository = invitationRepository;
            _plantSetter = plantSetter;
            _unitOfWork = unitOfWork;
            _telemetryClient = telemetryClient;
        }

        public async Task ProcessMessageAsync(PcsTopic pcsTopic, Message message, CancellationToken token)
        {
            var messageJson = Encoding.UTF8.GetString(message.Body);

            switch (pcsTopic)
            {
                case PcsTopic.Project:
                    var projectEvent = JsonSerializer.Deserialize<ProjectTopic>(messageJson);
                    _plantSetter.SetPlant(projectEvent.ProjectSchema);
                    ProcessProjectEvent(projectEvent);
                    break;

                case PcsTopic.CommPkg:
                    var commPkgEvent = JsonSerializer.Deserialize<CommPkgTopic>(messageJson);
                    _plantSetter.SetPlant(commPkgEvent.ProjectSchema);
                    ProcessCommPkgEvent(commPkgEvent);
                    break;

                case PcsTopic.McPkg:
                    var mcPkgEvent = JsonSerializer.Deserialize<McPkgTopic>(messageJson);
                    _plantSetter.SetPlant(mcPkgEvent.ProjectSchema);
                    ProcessMcPkgEvent(mcPkgEvent);
                    break;
            }
            await _unitOfWork.SaveChangesAsync(token);
        }

        private void ProcessMcPkgEvent(McPkgTopic mcPkgEvent)
        {
            if (!string.IsNullOrWhiteSpace(mcPkgEvent.CommPkgNoOld) || !string.IsNullOrWhiteSpace(mcPkgEvent.McPkgNoOld))
            {
                _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                    new Dictionary<string, string>
                    {
                        {BusReceiverTelemetryConstants.Event, mcPkgEvent.TopicName},
                        {BusReceiverTelemetryConstants.McPkgNo, mcPkgEvent.McPkgNo},
                        {BusReceiverTelemetryConstants.McPkgNoOld, mcPkgEvent.McPkgNoOld},
                        {BusReceiverTelemetryConstants.CommPkgNo, mcPkgEvent.CommPkgNo},
                        {BusReceiverTelemetryConstants.CommPkgNoOld, mcPkgEvent.CommPkgNoOld},
                        {BusReceiverTelemetryConstants.ProjectSchema, mcPkgEvent.ProjectSchema[4..]},
                        {BusReceiverTelemetryConstants.ProjectName, mcPkgEvent.ProjectName.Replace('$', '_')}
                    });

                _invitationRepository.MoveMcPkg(
                    mcPkgEvent.ProjectName,
                    mcPkgEvent.CommPkgNoOld,
                    mcPkgEvent.CommPkgNo,
                    mcPkgEvent.McPkgNoOld,
                    mcPkgEvent.McPkgNo,
                    mcPkgEvent.Description);
            }
            else
            {
                _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                    new Dictionary<string, string>
                    {
                        {BusReceiverTelemetryConstants.Event, mcPkgEvent.TopicName},
                        {BusReceiverTelemetryConstants.McPkgNo, mcPkgEvent.McPkgNo},
                        {BusReceiverTelemetryConstants.ProjectSchema, mcPkgEvent.ProjectSchema[4..]},
                        {BusReceiverTelemetryConstants.ProjectName, mcPkgEvent.ProjectName.Replace('$', '_')}
                    });

                _invitationRepository.UpdateMcPkgOnInvitations(
                    mcPkgEvent.ProjectName,
                    mcPkgEvent.McPkgNo,
                    mcPkgEvent.Description);
            }
        }

        private void ProcessCommPkgEvent(CommPkgTopic commPkgEvent)
        {
            if (!string.IsNullOrWhiteSpace(commPkgEvent.ProjectNameOld))
            {
                _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                    new Dictionary<string, string>
                    {
                        {BusReceiverTelemetryConstants.Event, commPkgEvent.TopicName},
                        {BusReceiverTelemetryConstants.CommPkgNo, commPkgEvent.CommPkgNo},
                        {BusReceiverTelemetryConstants.ProjectSchema, commPkgEvent.ProjectSchema[4..]},
                        {BusReceiverTelemetryConstants.ProjectNameOld, commPkgEvent.ProjectNameOld.Replace('$', '_')},
                        {BusReceiverTelemetryConstants.ProjectName, commPkgEvent.ProjectName.Replace('$', '_')}
                    });
                _invitationRepository.MoveCommPkg(
                    commPkgEvent.ProjectNameOld,
                    commPkgEvent.ProjectName,
                    commPkgEvent.CommPkgNo,
                    commPkgEvent.Description);
            }
            else
            {
                _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                    new Dictionary<string, string>
                    {
                        {BusReceiverTelemetryConstants.Event, commPkgEvent.TopicName},
                        {BusReceiverTelemetryConstants.CommPkgNo, commPkgEvent.CommPkgNo},
                        {BusReceiverTelemetryConstants.ProjectSchema, commPkgEvent.ProjectSchema[4..]},
                        {BusReceiverTelemetryConstants.ProjectName, commPkgEvent.ProjectName.Replace('$', '_')}
                    });
                _invitationRepository.UpdateCommPkgOnInvitations(
                    commPkgEvent.ProjectName,
                    commPkgEvent.CommPkgNo,
                    commPkgEvent.Description);
            }
        }

        private void ProcessProjectEvent(ProjectTopic projectEvent)
        {
            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {BusReceiverTelemetryConstants.Event, projectEvent.TopicName},
                    {BusReceiverTelemetryConstants.ProjectSchema, projectEvent.ProjectSchema[4..]},
                    {BusReceiverTelemetryConstants.ProjectName, projectEvent.ProjectName.Replace('$', '_')}
                });

            _invitationRepository.UpdateProjectOnInvitations(
                projectEvent.ProjectName,
                projectEvent.Description);
        }
    }
}
