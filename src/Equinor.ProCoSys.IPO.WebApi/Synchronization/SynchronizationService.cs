using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.Time;
using Equinor.ProCoSys.IPO.MainApi.Certificate;
using Equinor.ProCoSys.IPO.MainApi.Plant;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using Equinor.ProCoSys.IPO.WebApi.Telemetry;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public class SynchronizationService : ISynchronizationService
    {
        private readonly Guid _synchronizationUserOid;
        private readonly ILogger<SynchronizationService> _logger;
        private readonly ITelemetryClient _telemetryClient;
        private readonly IMediator _mediator;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IPlantSetter _plantSetter;
        private readonly ICurrentUserSetter _currentUserSetter;
        private readonly IBearerTokenSetter _bearerTokenSetter;
        private readonly IClaimsTransformation _claimsTransformation;
        private readonly IApplicationAuthenticator _authenticator;
        private readonly IPlantCache _plantCache;
        private readonly IOptionsMonitor<SynchronizationOptions> _options;
        private readonly ICertificateApiService _certificateApiService;

        public SynchronizationService(
            ILogger<SynchronizationService> logger,
            ITelemetryClient telemetryClient,
            IMediator mediator,
            IClaimsProvider claimsProvider,
            IPlantSetter plantSetter,
            ICurrentUserSetter currentUserSetter,
            IBearerTokenSetter bearerTokenSetter,
            IClaimsTransformation claimsTransformation,
            IApplicationAuthenticator authenticator,
            IPlantCache plantCache,
            IOptionsMonitor<SynchronizationOptions> options,
            ICertificateApiService certificateApiService)
        {
            _logger = logger;
            _telemetryClient = telemetryClient;
            _mediator = mediator;
            _claimsProvider = claimsProvider;
            _currentUserSetter = currentUserSetter;
            _claimsTransformation = claimsTransformation;
            _plantSetter = plantSetter;
            _authenticator = authenticator;
            _bearerTokenSetter = bearerTokenSetter;
            _plantCache = plantCache;
            _options = options;
            _certificateApiService = certificateApiService;
            _synchronizationUserOid = options.CurrentValue.UserOid;
        }

        public async Task Synchronize(CancellationToken cancellationToken)
        {
            var bearerToken = await _authenticator.GetBearerTokenForApplicationAsync();
            _bearerTokenSetter.SetBearerToken(bearerToken, false);

            _currentUserSetter.SetCurrentUser(_synchronizationUserOid);

            var currentUser = _claimsProvider.GetCurrentUser();
            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimsExtensions.OidType, _synchronizationUserOid.ToString()));
            currentUser.AddIdentity(claimsIdentity);

            foreach (var plant in await _plantCache.GetPlantIdsForUserOidAsync(_synchronizationUserOid))
            {
                _logger.LogInformation($"Synchronizing plant {plant}...");

                _plantSetter.SetPlant(plant);
                await _claimsTransformation.TransformAsync(currentUser);

                var startTime = TimeService.UtcNow;
                
                // Synchronize here

                var endTime = TimeService.UtcNow;

                _logger.LogInformation($"Plant {plant} synchronized. Duration: {(endTime - startTime).TotalSeconds}s.");
                _telemetryClient.TrackMetric("Synchronization Time", (endTime - startTime).TotalSeconds, "Plant", plant);
            }
        }
    }
}
