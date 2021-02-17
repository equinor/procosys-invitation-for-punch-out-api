using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.PersonCommands.CreateSavedFilter;
using Equinor.ProCoSys.IPO.Command.PersonCommands.DeleteSavedFilter;
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
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromBody] CreateSavedFilterDto dto)
        {
            var result = await _mediator.Send(new CreateSavedFilterCommand(dto.ProjectName, dto.Title, dto.Criteria, dto.DefaultFilter));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpDelete("SavedFilters/{id}")]
        public async Task<ActionResult> DeleteSavedFilter(
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
