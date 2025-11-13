using System.Text.Json;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;
using Equinor.ProCoSys.IPO.MessageContracts;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Fam;

public class SendAllDataToFamCommandHandler : IRequestHandler<SendAllDataToFamCommand, Result<string>>
{
    private readonly IFamRepository _famRepository;
    private readonly IOptions<FamOptions> _famOptions;
    private readonly IFamCredential _famCredential;
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly ILogger _logger;

    public SendAllDataToFamCommandHandler(IFamRepository famRepository,
        IFamCredential famCredential,
        IOptions<FamOptions> famOptions,
        IEventHubProducerService eventHubProducerService,
        ILogger<SendAllDataToFamCommandHandler> logger)
    {
        _famRepository = famRepository;
        _famOptions = famOptions;
        _famCredential = famCredential;
        _eventHubProducerService = eventHubProducerService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SendAllDataToFamCommand request, CancellationToken cancellationToken)
    {
        var mapper = CreateCommonLibMapper();

        var statusResult = string.Empty;

        statusResult += await SendEventsToFam<ICommPkgEventV1>(
            _famRepository.GetCommPkgs, CommonLibClassConstants.CommPkg, mapper);

        statusResult += await SendEventsToFam<IParticipantEventV1>(
            _famRepository.GetParticipants, CommonLibClassConstants.Participant, mapper);

        statusResult += await SendEventsToFam<IInvitationEventV1>(
            _famRepository.GetInvitations, CommonLibClassConstants.Invitation, mapper);

        statusResult += await SendEventsToFam<ICommentEventV1>(
            _famRepository.GetComments, CommonLibClassConstants.Comment, mapper);

        statusResult += await SendEventsToFam<IMcPkgEventV1>(
            _famRepository.GetMcPkgs, CommonLibClassConstants.McPkg, mapper);

        return new SuccessResult<string>(statusResult);
    }

    private async Task<string> SendEventsToFam<T>(
        Func<Task<IEnumerable<T>>> getEvents,
        string commonLibClassName,
        SchemaMapper mapper)
    {
        try
        {
            _logger.LogInformation($"Starting to process events for type {commonLibClassName} to send to FAM");
            var batchSize = _famOptions.Value.BatchSize == default ? 5000 : _famOptions.Value.BatchSize;
            var totalSent = 0;

            var allEvents = await getEvents();

            foreach (var batch in allEvents.Batch(batchSize))
            {
                var eventBatch = batch.ToList();

                _logger.LogInformation($"Processing batch of {eventBatch.Count} events for type {commonLibClassName} to send to FAM");

                if (eventBatch.Count == 0)
                {
                    _logger.LogInformation($"Found no more events for {commonLibClassName}");
                    continue;
                }

                var eventsAsJson = eventBatch.Select(e => JsonSerializer.Serialize(e)).ToList();
                var messages = eventsAsJson.SelectMany(e => TieMapper.CreateTieMessage(e, commonLibClassName));
                var commonLibMappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development" && commonLibMappedMessages.Count > 0)
                {
                    _logger.LogInformation($"Sending batch of {commonLibMappedMessages.Count} events for type {commonLibClassName} to FAM.");
                    await SendFamMessages(commonLibMappedMessages);
                    totalSent += commonLibMappedMessages.Count;
                }
            }

            var successFullySentMessage = $"Successfully sent {totalSent} events for type {commonLibClassName} to FAM.\n";
            _logger.LogInformation(successFullySentMessage);
            return successFullySentMessage;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Failed to transfer data to FAM for type {commonLibClassName}.";
            _logger.LogError(ex, errorMessage);
            return errorMessage + " See logs for more details.";
        }
    }

    private SchemaMapper CreateCommonLibMapper()
    {
        ISchemaSource source = new ApiSource(new ApiSourceOptions(), _famCredential.GetToken());

        // Adds caching functionality
        source = new CacheWrapper(
            source,
            maxCacheAge: TimeSpan.FromDays(1), // Use TimeSpan.Zero for no recache based on age
            checkForChangesFrequency: TimeSpan
                .FromHours(1)); // Use TimeSpan.Zero when cache should never check for changes.

        var mapper = new SchemaMapper("ProCoSys_Events", "FAM", source);
        return mapper;
    }

    private async Task SendFamMessages(IEnumerable<Message> messages)
    {
        try
        {
            await _eventHubProducerService.SendDataAsync(messages);
        }
        catch (Exception e)
        {
            throw new Exception("Error: Could not send message.", e);
        }
    }
}
