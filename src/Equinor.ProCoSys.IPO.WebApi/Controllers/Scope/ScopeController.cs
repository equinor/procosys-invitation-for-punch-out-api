using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Equinor.ProCoSys.IPO.Query.GetProjectsInPlant;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = Permissions.COMMPKG_READ)]
        [HttpGet("/CommPkgs")]
        public async Task<ActionResult<List<ProCoSysCommPkgDto>>> GetCommPkgsInProject(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] int projectId,
            [FromQuery] string startsWithCommPkgNo)
        {
            var result = await _mediator.Send(new GetCommPkgsInProjectQuery(projectId, startsWithCommPkgNo));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets Projects from ProCoSys main API by Plant
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>All ProCoSys projects (which have any commpkgs) in given plant</returns>
        [Authorize(Roles = Permissions.PROJECT_READ)]
        [HttpGet("/Projects")]
        public async Task<ActionResult<List<ProCoSysProjectDto>>> GetProjectsInPlant(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant)
        {
            var result = await _mediator.Send(new GetProjectsInPlantQuery());
            return this.FromResult(result);
        }
    }
}
