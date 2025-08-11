using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter;
using Equinor.ProCoSys.IPO.Command.PersonCommands.DeleteSavedFilter;
using Equinor.ProCoSys.IPO.Command.PersonCommands.UpdateSavedFilter;
using Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Persons
{
    [ApiController]
    [Route("Persons")]
    public class PersonsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PersonsController(IMediator mediator) => _mediator = mediator;

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpPost("SavedFilter")]
        public async Task<ActionResult<int>> CreateSavedFilter(
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060: Remove unused parameter")]
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromBody] CreateSavedFilterDto dto)
        {
            var result = await _mediator.Send(new CreateSavedFilterCommand(dto.ProjectName, dto.Title, dto.Criteria, dto.DefaultFilter));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("SavedFilters")]
        public async Task<ActionResult<List<SavedFilterDto>>> GetSavedFiltersInProject(
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060: Remove unused parameter")]
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] string projectName)
        {
            var result = await _mediator.Send(new GetSavedFiltersInProjectQuery(projectName));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpPut("SavedFilters/{id}")]
        public async Task<ActionResult> UpdateSavedFilter(
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060: Remove unused parameter")]
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromRoute] int id,
            [FromBody] UpdateSavedFilterDto dto)
        {
            var command = new UpdateSavedFilterCommand(
                id,
                dto.Title,
                dto.Criteria,
                dto.DefaultFilter,
                dto.RowVersion);

            var result = await _mediator.Send(command);
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpDelete("SavedFilters/{id}")]
        public async Task<ActionResult> DeleteSavedFilter(
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060: Remove unused parameter")]
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromRoute] int id,
            [FromBody] DeleteSavedFilterDto dto)
        {
            var result = await _mediator.Send(new DeleteSavedFilterCommand(id, dto.RowVersion));
            return this.FromResult(result);
        }
    }
}
