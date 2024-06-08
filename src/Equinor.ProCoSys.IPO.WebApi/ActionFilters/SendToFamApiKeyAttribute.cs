using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Fam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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
        if (famOptions is null)
        {
            context.Result = new ForbidResult("Failed to retrieve configuration");
            return;
        }

        var famApiKeyFromConfig = famOptions.CurrentValue.SendToFamApiKey;

        if (string.IsNullOrWhiteSpace(famApiKeyFromConfig))
        {
            context.Result = new ForbidResult($"Api key {APIKEYNAME} is invalid, missing configuration."); 
            return;
        }


        if (famApiKeyFromConfig != apiKeyFromRequest)
        {
            context.Result = new ForbidResult($"Api key {APIKEYNAME} is invalid"); 
            return;
        }

        await next(); // proceed to the action if API key is valid
    }
}
