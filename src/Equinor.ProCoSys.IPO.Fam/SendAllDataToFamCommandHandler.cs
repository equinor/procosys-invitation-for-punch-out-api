using System.Text.Json;
using Equinor.ProCoSys.FamWebJob.Core.Mappers;
using Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam;
using Equinor.TI.Common.Messaging;
using Equinor.TI.CommonLibrary.Mapper;
using Equinor.TI.CommonLibrary.Mapper.Core;
using Fam.Core.EventHubs.Contracts;
using Fam.Models.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Fam;

public class SendAllDataToFamCommandHandler : IRequestHandler<SendAllDataToFamCommand, Result<string>>
{
    private readonly IFamRepository _famRepository;
    private readonly CommonLibConfig _commonLibConfig;
    private readonly IEventHubProducerService _eventHubProducerService;

    public SendAllDataToFamCommandHandler(IFamRepository famRepository, IOptions<CommonLibConfig> commonLibConfig, IEventHubProducerService eventHubProducerService)
    {
        _famRepository = famRepository;
        _commonLibConfig = commonLibConfig.Value;
        _eventHubProducerService = eventHubProducerService;
    }

    public async Task<Result<string>> Handle(SendAllDataToFamCommand request, CancellationToken cancellationToken)
    {
        var participants = (await _famRepository.GetParticipants()).ToList();
        var participantsAsJson = participants.Select(e => JsonSerializer.Serialize(e)).ToList();
        var messages = participantsAsJson.SelectMany(e => TieMapper.CreateTieMessage(e, "IpoInvitationParticipant"));
        var mapper = CreateCommonLibMapper();
        var mappedMessages = messages.Select(m => mapper.Map(m).Message).Where(m => m.Objects.Any()).ToList();
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
        {
            await SendFamMessages(mappedMessages);
        }
        return new SuccessResult<string>("");
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
