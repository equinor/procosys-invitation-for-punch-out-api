using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.MeetingCommands.CreateMeeting;
using Equinor.ProCoSys.IPO.Query.GetMeetingById;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Meeting
{
    [ApiController]
    [Route("Meetings")]
    public class MeetingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MeetingsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<MeetingDto>> GetMeetingById(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] Guid meetingId)
        {
            var result = await _mediator.Send(new GetMeetingByIdQuery(meetingId));
            return this.FromResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateMeeting(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromBody] CreateMeetingDto dto)
        {
            var result = await _mediator.Send(new CreateMeetingCommand());
            return this.FromResult(result);
        }
    }
}
