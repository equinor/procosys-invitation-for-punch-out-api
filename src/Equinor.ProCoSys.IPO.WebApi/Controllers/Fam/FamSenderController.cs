using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Fam;
using Equinor.ProCoSys.IPO.WebApi.ActionFilters;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Fam;

[ApiController]
[Route("FamSender")]
public class FamSenderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FamSenderController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Sends data to FAM
    /// </summary>
    /// <param name="plant"></param>
    /// <returns>Result of sending to FAM</returns>
    [Authorize()]
    //TODO: JSOI Add ApiKey authentication with action filter
    [SendToFamApiKey]
    [HttpPost("SendAllData")]
    public async Task<ActionResult<string>> SendAllDataToFam(
        [FromHeader(Name = "X-Send-To-Fam-Api-Key")] //TODO: Move to constant
        [Required]
        string apiKey) 
    {
        var result = await _mediator.Send(new SendAllDataToFamCommand());
        return this.FromResult(result);
    }
}
