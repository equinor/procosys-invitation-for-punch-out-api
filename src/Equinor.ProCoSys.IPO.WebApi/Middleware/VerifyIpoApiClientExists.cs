using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.IPO.WebApi.Authentication;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.Middleware
{
    public class VerifyIpoApiClientExists : IHostedService
    {
        private readonly IServiceScopeFactory _serviceProvider;
        private readonly IOptionsMonitor<IpoAuthenticatorOptions> _options;
        private readonly ILogger<VerifyIpoApiClientExists> _logger;

        public VerifyIpoApiClientExists(
            IServiceScopeFactory serviceProvider,
            IOptionsMonitor<IpoAuthenticatorOptions> options, 
            ILogger<VerifyIpoApiClientExists> logger)
        {
            _serviceProvider = serviceProvider;
            _options = options;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var mediator =
                scope.ServiceProvider
                    .GetRequiredService<IMediator>();
            var currentUserSetter =
                scope.ServiceProvider
                    .GetRequiredService<ICurrentUserSetter>();

            var oid = _options.CurrentValue.IpoApiObjectId;
            _logger.LogInformation($"Ensuring '{oid}' exists as Person");
            try
            {
                currentUserSetter.SetCurrentUserOid(oid);
                await mediator.Send(new CreatePersonCommand(oid), cancellationToken);
                _logger.LogInformation($"'{oid}' ensured");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception handling {nameof(CreatePersonCommand)}", e);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
