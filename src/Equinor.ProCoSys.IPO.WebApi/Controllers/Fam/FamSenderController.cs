using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Fam;
using Equinor.ProCoSys.IPO.WebApi.ActionFilters;
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
    /// <param name="apiKey"></param>
    /// <returns>Result of sending to FAM</returns>
    [Authorize()]
    [SendToFamApiKey]
    [HttpPost("SendAllData")]
    public async Task<ActionResult<string>> SendAllDataToFam(
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060: Remove unused parameter")]
        [FromHeader(Name = SendToFamApiKeyAttribute.FamApiKeyHeader)]
        [Required]
        string apiKey)
    {
        var result = await _mediator.Send(new SendAllDataToFamCommand());
        return this.FromResult(result);
    }
}
