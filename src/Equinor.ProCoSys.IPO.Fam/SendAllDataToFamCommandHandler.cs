using System.Text.Json;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;
using Equinor.ProCoSys.IPO.MessageContracts;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Fam;

public class SendAllDataToFamCommandHandler : IRequestHandler<SendAllDataToFamCommand, Result<string>>
{
    private readonly IFamRepository _famRepository;
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;
    private readonly ILogger _logger;

    public SendAllDataToFamCommandHandler(IFamRepository famRepository,
        IOptions<CommonLibConfig> commonLibConfig,
        IEventHubProducerService eventHubProducerService,
        ILogger<SendAllDataToFamCommandHandler> logger)
    {
        _famRepository = famRepository;
        _commonLibConfig = commonLibConfig.Value;
        _eventHubProducerService = eventHubProducerService;
        _logger = logger;
    }

    public async Task<Result<string>> Handle(SendAllDataToFamCommand request, CancellationToken cancellationToken)
    {
        var mapper = CreateCommonLibMapper();

        var statusResult = string.Empty;

        statusResult += await SendEventsToFam<IParticipantEventV1>( 
            _famRepository.GetParticipants, CommonLibClassConstants.Participant, mapper);

        statusResult += await SendEventsToFam<IInvitationEventV1>(
            _famRepository.GetInvitations, CommonLibClassConstants.Invitation, mapper);

        statusResult += await SendEventsToFam<ICommentEventV1>(
            _famRepository.GetComments, CommonLibClassConstants.Comment, mapper);

        statusResult += await SendEventsToFam<IMcPkgEventV1>(
            _famRepository.GetMcPkgs, CommonLibClassConstants.McPkg, mapper);

        statusResult += await SendEventsToFam<ICommPkgEventV1>(
            _famRepository.GetCommPkgs, CommonLibClassConstants.CommPkg, mapper);

        return new SuccessResult<string>(statusResult);
    }

    private async Task<string> SendEventsToFam<T>(
        Func<Task<IEnumerable<T>>> getEvents,
        string commonLibClassName,
        SchemaMapper mapper)
    {
        try
        {
            var events = (await getEvents()).ToList();

            _logger.LogInformation($"Found {events.Count} events for type {commonLibClassName} to send to FAM");
            
            if (!events.Any())
            {
                return $"Found no events for {commonLibClassName}";
            }

            var eventsAsJson = events.Select(e => JsonSerializer.Serialize(e)).ToList();
            var messages = eventsAsJson.SelectMany(e => TieMapper.CreateTieMessage(e, commonLibClassName));
            var commonLibMappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            {
                _logger.LogInformation($"Sending {commonLibMappedMessages.Count} events for type {commonLibClassName} to FAM.");
                await SendFamMessages(commonLibMappedMessages);
            }

            return $"Successfully sent {commonLibMappedMessages.Count} events for type {commonLibClassName} to FAM.\n";
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
        ISchemaSource source = new ApiSource(new ApiSourceOptions
        {
            TokenProviderConnectionString = "RunAs=App;" +
            $"AppId={_commonLibConfig.ClientId};" +
                                            $"TenantId={_commonLibConfig.TenantId};" +
                                            $"AppKey={_commonLibConfig.ClientSecret}"
        });

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
        catch (FamConfigException e)
        {
            throw new Exception("Configuration error: Could not send message.", e);
        }
        catch (Exception e)
        {
            throw new Exception("Error: Could not send message.", e);
        }
    }
}
