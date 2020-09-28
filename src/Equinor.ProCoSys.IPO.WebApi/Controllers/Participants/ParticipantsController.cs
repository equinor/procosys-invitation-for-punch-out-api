using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Participants
{
    [ApiController]
    [Route("FunctionalRoles")]
    public class ParticipantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParticipantsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets functional roles from ProCoSys library API by classification
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="classification"></param>
        /// <returns>All ProCoSys functional roles which are of the given classification</returns>
        [Authorize(Roles = Permissions.LIBRARY_FUNCTIONAL_ROLE_READ)]
        [HttpGet("/FunctionalRoles")]
        public async Task<ActionResult<List<ProCoSysFunctionalRoleDto>>> GetFunctionalRoles(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            [FromQuery] string classification)
        {
            var result = await _mediator.Send(new GetFunctionalRolesQuery(classification));
            return this.FromResult(result);
        }
    }
}
