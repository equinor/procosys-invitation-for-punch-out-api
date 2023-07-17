using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Middleware
{
    public class PersonValidatorMiddleware
    {
        private readonly RequestDelegate _next;

        public PersonValidatorMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            ICurrentUserProvider currentUserProvider,
            IPersonCache personCache,
            ILogger<PersonValidatorMiddleware> logger)
        {
            logger.LogDebug("----- {MiddlewareName} start", GetType().Name);
            if (currentUserProvider.HasCurrentUser)
            {
                var oid = currentUserProvider.GetCurrentUserOid();
                if (!await personCache.ExistsAsync(oid))
                {
                    await context.WriteForbidden(logger);
                    return;
                }
            }

            logger.LogDebug("----- {MiddlewareName} complete", GetType().Name);
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
