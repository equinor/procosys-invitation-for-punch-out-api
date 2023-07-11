using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateRfocAcceptedStatus;
using Equinor.ProCoSys.PcsServiceBus;
using Equinor.ProCoSys.PcsServiceBus.Topics;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public class CertificateEventProcessorService : ICertificateEventProcessorService
    {
        private readonly ILogger _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IMediator _mediator;
        private readonly IPlantSetter _plantSetter;

        private const string IpoBusReceiverTelemetryEvent = "IPO Bus Receiver";

        public CertificateEventProcessorService(
            ILogger<CertificateEventProcessorService> logger,
            ITelemetryClient telemetryClient,
            IMediator mediator,
            IPlantSetter plantSetter)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _mediator = mediator;
            _plantSetter = plantSetter;
        }

        public async Task ProcessCertificateEventAsync(string messageJson)
        {
            var certificateEvent = JsonSerializer.Deserialize<CertificateTopic>(messageJson);
            if (certificateEvent != null && certificateEvent.Behavior == "delete")
            {
                TrackUnsupportedDeleteEvent(PcsTopic.Certificate, certificateEvent.ProCoSysGuid);
                return;
            }

            if (certificateEvent != null && (
                certificateEvent.Plant.IsEmpty() ||
                certificateEvent.CertificateNo.IsEmpty()))
            {
                throw new ArgumentNullException($"Deserialized JSON is not a valid CertificateEvent {messageJson}");
            }

            _plantSetter.SetPlant(certificateEvent.Plant);

            TrackCertificateEvent(certificateEvent);

            await HandleRfocAcceptedIfRelevantAsync(certificateEvent);
        }

        private async Task HandleRfocAcceptedIfRelevantAsync(CertificateTopic certificateEvent)
        {
            if (certificateEvent.CertificateType == "RFOC")
            {
                var result = await _mediator.Send(new UpdateRfocAcceptedCommand(
                    certificateEvent.ProjectName,
                    certificateEvent.ProCoSysGuid));

                LogRfocAcceptedResult(certificateEvent, result);
            }
        }

        private void LogRfocAcceptedResult(CertificateTopic certificateEvent, Result<Unit> result)
        {
            var resultOk = result.ResultType == ResultType.Ok;

            _logger.LogInformation(resultOk ? "RfocAccepted handling complete." : "RfocAccepted handling functions failed.");

            var telemetryDictionary = new Dictionary<string, string>
            {
                {"Status", resultOk ? "Succeeded" : "Failed"},
                {"Plant", certificateEvent.Plant},
                {"Type", "UpdateRfocStatus"},
                {"ProjectName", string.IsNullOrWhiteSpace(certificateEvent.ProjectName) ? "null or empty" : certificateEvent.ProjectName},
                {"CertificateNo", certificateEvent.CertificateNo},
                {"CertificateType", certificateEvent.CertificateType}
            };

            _telemetryClient.TrackEvent("Synchronization Status", telemetryDictionary);
        }

        private void TrackCertificateEvent(CertificateTopic certificateTopic) =>
            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {"Event", CertificateTopic.TopicName},
                    {nameof(certificateTopic.CertificateNo), certificateTopic.CertificateNo},
                    {nameof(certificateTopic.CertificateType), certificateTopic.CertificateType},
                    {nameof(certificateTopic.CertificateStatus), certificateTopic.CertificateStatus.ToString()},
                    {nameof(certificateTopic.Plant), NormalizePlant(certificateTopic.Plant)},
                    {nameof(certificateTopic.ProjectName), NormalizeProjectName(certificateTopic.ProjectName)}
                });

        private void TrackUnsupportedDeleteEvent(PcsTopic topic, Guid guid) =>
            _telemetryClient.TrackEvent(IpoBusReceiverTelemetryEvent,
                new Dictionary<string, string>
                {
                    {"Event Delete", topic.ToString()},
                    {"ProCoSysGuid", guid.ToString()},
                    {"Supported", "false"}
                });

        private string NormalizePlant(string plant) => plant[4..];

        private string NormalizeProjectName(string projectName) => string.IsNullOrWhiteSpace(projectName) ? null : projectName.Replace('$', '_');
    }
}
