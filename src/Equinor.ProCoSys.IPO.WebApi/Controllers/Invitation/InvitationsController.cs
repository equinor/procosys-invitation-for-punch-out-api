using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    [ApiController]
    [Route("Invitations")]
    public class InvitationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvitationsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{id}")]
        public async Task<ActionResult<int>> GetInvitationById(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromRoute] int id)
        {
            var result = await _mediator.Send(new GetInvitationByIdQuery(id));
            return this.FromResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromBody] CreateInvitationDto dto)
        {
            var result = await _mediator.Send(
                new CreateInvitationCommand(
                    new CreateMeetingCommand(dto.Meeting.Title, dto.Meeting.BodyHtml, dto.Meeting.Location, dto.Meeting.StartTime, dto.Meeting.EndTime, dto.Meeting.ParticipantOids)));
            return this.FromResult(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<int>> EditInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromRoute] int id,
            [FromBody] EditInvitationDto dto)
        {
            var result = await _mediator.Send(
                new EditInvitationCommand(
                    id,
                    new EditMeetingCommand(dto.Meeting.Title, dto.Meeting.BodyHtml, dto.Meeting.Location, dto.Meeting.StartTime, dto.Meeting.EndTime, dto.Meeting.ParticipantOids)));
            return this.FromResult(result);
        }
    }
}
