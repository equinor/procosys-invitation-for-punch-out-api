using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Middleware
{
    public class VerifyOidInDbMiddleware
    {
        private readonly RequestDelegate _next;

        public VerifyOidInDbMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            IHttpContextAccessor httpContextAccessor,
            IMediator mediator,
            ILogger<VerifyOidInDbMiddleware> logger)
        {
            logger.LogInformation($"----- {GetType().Name} start");
            var httpContextUser = httpContextAccessor.HttpContext.User;
            var oid = httpContextUser.Claims.TryGetOid();
            if (oid.HasValue)
            {
                var givenName = httpContextUser.Claims.TryGetGivenName();
                var surName = httpContextUser.Claims.TryGetSurName();

                var command = new CreatePersonCommand(oid.Value, givenName, surName);
                try
                {
                    await mediator.Send(command);
                }
                catch (Exception e)
                {
                    // We have to do this silently as concurrency is a very likely problem.
                    // For a user accessing the application for the first time, there will probably be multiple
                    // requests in parallel.
                    logger.LogError($"Exception handling {nameof(CreatePersonCommand)}", e);
                }
            }
            
            logger.LogInformation($"----- {GetType().Name} complete");
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
