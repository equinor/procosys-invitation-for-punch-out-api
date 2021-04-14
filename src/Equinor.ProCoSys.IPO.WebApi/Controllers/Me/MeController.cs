using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetOutstandingIpos;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Me
{
    [ApiController]
    [Route("Me")]
    public class MeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MeController(IMediator mediator) => _mediator = mediator;

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("OutstandingIpos")]
        public async Task<ActionResult<OutstandingIposResultDto>> GetOutstandingIpos(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] string projectName)
        {
            var result = await _mediator.Send(new GetOutstandingIposForCurrentPersonQuery(projectName));
            return this.FromResult(result);
        }
    }
}
