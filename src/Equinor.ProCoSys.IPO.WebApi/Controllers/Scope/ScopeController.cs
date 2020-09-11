using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Scope
{
    [ApiController]
    [Route("CommPkgs")]
    public class ScopeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScopeController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets CommPkgs from ProCoSys main API by CommPkgNos
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="projectId"></param>
        /// <param name="startsWithCommPkgNo"></param>
        /// <returns>All ProCoSys commpkgs that match the search parameters</returns>
        //Find out what we're doing about permissions
        [HttpGet("/CommPkgs")]
        public async Task<ActionResult<List<ProcosysCommPkgDto>>> GetCommPkgsInProject(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] int projectId,
            [FromQuery] string startsWithCommPkgNo)
        {
            var result = await _mediator.Send(new GetCommPkgsInProjectQuery(projectId, startsWithCommPkgNo));
            return this.FromResult(result);
        }
    }
}
