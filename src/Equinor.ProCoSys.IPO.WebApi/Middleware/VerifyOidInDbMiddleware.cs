using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authorization;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson;
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
            logger.LogDebug("----- {MiddlewareName} start", GetType().Name);
            if (httpContextAccessor.HttpContext != null)
            {
                var httpContextUser = httpContextAccessor.HttpContext.User;
                var oid = httpContextUser.Claims.TryGetOid();
                if (oid.HasValue)
                {
                    var command = new CreatePersonCommand(oid.Value);
                    try
                    {
                        await mediator.Send(command);
                    }
                    catch (Exception e)
                    {
                        // We have to do this silently as concurrency is a very likely problem.
                        // For a user accessing the application for the first time, there will probably be multiple
                        // requests in parallel.
                        logger.LogError(e, $"Exception handling {nameof(CreatePersonCommand)}");
                    }
                }
            }

            logger.LogDebug("----- {MiddlewareName} complete", GetType().Name);
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
