using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.IPO.WebApi.ActionFilters;

public class SendToFamApiKeyAttribute : Attribute, IAsyncActionFilter
{
    private const string APIKEYNAME = "X-Send-To-Fam-Api-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var apiKeyFromRequest))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var famOptions = context.HttpContext.RequestServices.GetRequiredService<IOptionsMonitor<FamOptions>>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<SendToFamApiKeyAttribute>>();
        
        if (famOptions is null)
        {
            logger.LogError($"Failed to retrieve configuration for {APIKEYNAME}");
            context.Result = new ForbidResult();
            return;
        }

        var famApiKeyFromConfig = famOptions.CurrentValue.SendToFamApiKey;

        if (string.IsNullOrWhiteSpace(famApiKeyFromConfig))
        {
            logger.LogError($"The configured value for Api key {APIKEYNAME} is empty.");
            context.Result = new ForbidResult(); 
            return;
        }


        if (famApiKeyFromConfig != apiKeyFromRequest)
        {
            logger.LogError($"The value sent in for Api key {APIKEYNAME} does not match configured value.");
            context.Result = new ForbidResult(); 
            return;
        }

        await next();
    }
}
