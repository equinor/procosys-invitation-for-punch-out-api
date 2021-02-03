using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreatePerson;
using Equinor.ProCoSys.IPO.WebApi.Authorizations;
using FluentValidation;
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
                var userName = httpContextUser.Claims.TryGetUserName();
                var email = httpContextUser.Claims.TryGetEmail();

                if (givenName == null || surName == null || userName == null || email == null)
                {
                    var givenNameClaim = httpContextUser.Claims.SingleOrDefault(x => x.Type == ClaimTypes.GivenName);
                    var surNameclaim = httpContextUser.Claims.SingleOrDefault(c => c.Type == ClaimTypes.Surname);
                    var nameClaim = httpContextUser.Claims.SingleOrDefault(c => c.Type == ClaimsExtensions.Name);
                    var upnClaim = httpContextUser.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Upn);
                    var unique_nameClaim = httpContextUser.Claims.SingleOrDefault(x => x.Type == ClaimsExtensions.UniqueName);
                    var unique_nameRenamedClaim = httpContextUser.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name);
                    logger.LogError($"Claim(s) not found. GivenName found: {givenNameClaim != null}, GivenName value: {givenNameClaim?.Value}. SurName found: {surNameclaim != null}, SurName value: {surNameclaim?.Value}. Name found: {nameClaim != null}, Name value: {nameClaim?.Value}.  UPN found: {upnClaim != null}, UPN value: {upnClaim?.Value}. Unique_name found: {unique_nameClaim != null}, Unique_name value: {unique_nameClaim?.Value}. Unique_name_renamed found: {unique_nameRenamedClaim != null}, Unique_name_renamed value: {unique_nameRenamedClaim?.Value}.");
                }

                var command = new CreatePersonCommand(oid.Value, givenName, surName, userName, email);
                try
                {
                    await mediator.Send(command);
                }
                catch (ValidationException)
                {
                    throw;
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
