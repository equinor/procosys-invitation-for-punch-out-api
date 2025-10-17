using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Common.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Middleware
{
    public class PlantValidatorMiddleware
    {
        private readonly RequestDelegate _next;

        public PlantValidatorMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(
            HttpContext context,
            IPlantProvider plantProvider,
            IPermissionCache permissionCache,
            ILogger<PlantValidatorMiddleware> logger)
        {
            logger.LogDebug("----- {MiddlewareName} start", GetType().Name);
            var plantId = plantProvider.Plant;
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated && plantId != null)
            {
                if (!await permissionCache.IsAValidPlantForCurrentUserAsync(plantId, CancellationToken.None))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        {CurrentPlantMiddleware.PlantHeader, new[] {$"Plant '{plantId}' is not a valid plant"}}
                    };
                    await context.WriteBadRequestAsync(errors, logger);
                    return;
                }
            }

            logger.LogDebug("----- {MiddlewareName} complete", GetType().Name);
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
