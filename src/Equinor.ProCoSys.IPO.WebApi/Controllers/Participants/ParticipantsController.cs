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
        /// Gets functional roles from ProCoSys library API with IPO classification 
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>All ProCoSys functional roles which have classification IPO</returns>
        [Authorize(Roles = Permissions.LIBRARY_FUNCTIONAL_ROLE_READ)]
        [HttpGet("/FunctionalRoles")]
        public async Task<ActionResult<List<ProCoSysFunctionalRoleDto>>> GetFunctionalRolesForIpo(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant)
        {
            const string classification = "IPO";
            var result = await _mediator.Send(new GetFunctionalRolesForIpoQuery(classification));
            return this.FromResult(result);
        }
    }
}
