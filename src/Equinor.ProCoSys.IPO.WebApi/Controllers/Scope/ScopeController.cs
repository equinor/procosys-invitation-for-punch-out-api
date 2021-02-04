using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject;
using Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject;
using Equinor.ProCoSys.IPO.Query.GetProjectsInPlant;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Scope
{
    [ApiController]
    [Route("Scope")]
    public class ScopeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ScopeController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets CommPkgs from ProCoSys main API by CommPkgNos
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="projectName"></param>
        /// <param name="startsWithCommPkgNo"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="currentPage"></param>
        /// <returns>All ProCoSys commpkgs that match the search parameters</returns>
        [Authorize(Roles = Permissions.COMMPKG_READ)]
        [HttpGet("CommPkgsV2")]
        public async Task<ActionResult<ProCoSysCommPkgSearchDto>> GetCommPkgsInProjectV2(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] string projectName,
            [FromQuery] string startsWithCommPkgNo,
            [FromQuery] int itemsPerPage = 10,
            [FromQuery] int currentPage = 0)
        {
            var result = await _mediator.Send(new GetCommPkgsInProjectQuery(projectName, startsWithCommPkgNo, itemsPerPage, currentPage));
            return this.FromResult(result);

        }

        /// <summary>
        /// Gets CommPkgs from ProCoSys main API by CommPkgNos
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="projectName"></param>
        /// <param name="startsWithCommPkgNo"></param>
        /// <returns>All ProCoSys commpkgs that match the search parameters</returns>
        [Authorize(Roles = Permissions.COMMPKG_READ)]
        [HttpGet("CommPkgs")]
        public async Task<IList<ProCoSysCommPkgDto>> GetCommPkgsInProject(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] string projectName,
            [FromQuery] string startsWithCommPkgNo)
        {
            var result = await _mediator.Send(new GetCommPkgsInProjectQuery(projectName, startsWithCommPkgNo, 10, 0));

            return result.Data.CommPkgs;
        }

        /// <summary>
        /// Gets Projects from ProCoSys main API by Plant
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>All ProCoSys projects (which have any commpkgs) in given plant</returns>
        [Authorize(Roles = Permissions.PROJECT_READ)]
        [HttpGet("Projects")]
        public async Task<ActionResult<List<ProCoSysProjectDto>>> GetProjectsInPlant(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant)
        {
            var result = await _mediator.Send(new GetProjectsInPlantQuery());
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets McPkgs from ProCoSys main API by McPkgNos
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="projectName"></param>
        /// <param name="commPkgNo"></param>
        /// <returns>All ProCoSys mcpkgs that match the search parameters</returns>
        [Authorize(Roles = Permissions.MCPKG_READ)]
        [HttpGet("McPkgs")]
        public async Task<ActionResult<List<ProCoSysMcPkgDto>>> GetMcPkgsInProject(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] string projectName,
            [FromQuery] string commPkgNo)
        {
            var result = await _mediator.Send(new GetMcPkgsUnderCommPkgInProjectQuery(projectName, commPkgNo));
            return this.FromResult(result);
        }
    }
}
