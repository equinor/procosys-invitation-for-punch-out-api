using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        // TODO: Add permissions
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

        // TODO: Add permissions
        [HttpPost]
        public async Task<ActionResult<int>> CreateInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromBody] CreateInvitationDto dto)
        {
            var mcPkgs = dto.McPkgScope?.Select(mc =>
                new McPkgScopeForCommand(mc.McPkgNo, mc.Description, mc.CommPkgNo)).ToList();
            var commPkgs = dto.CommPkgScope?.Select(c =>
                new CommPkgScopeForCommand(c.CommPkgNo, c.Description, c.Status)).ToList();
            var participants = dto.Participants.Select(p =>
                new ParticipantsForCommand(
                        p.Organization,
                        p.ExternalEmail,
                        p.Person != null
                            ? new PersonForCommand(p.Person.AzureOid ?? Guid.Empty, p.Person.FirstName, p.Person.LastName, p.Person.Email,
                                p.Person.Required)
                            : null,
                        p.FunctionalRole != null 
                            ? new FunctionalRoleForCommand(
                                p.FunctionalRole.Code,
                                p.FunctionalRole.Email,
                                p.FunctionalRole.UsePersonalEmail,
                                p.FunctionalRole.Persons?.Select(person =>
                                    new PersonForCommand(
                                        person.AzureOid ?? Guid.Empty,
                                        person.FirstName, 
                                        person.LastName, 
                                        person.Email,
                                        person.Required)).ToList()) 
                            : null,
                        p.SortKey)
            ).ToList();

            var result = await _mediator.Send(
                new CreateInvitationCommand(
                    dto.Title,
                    dto.BodyHtml,
                    dto.Location,
                    dto.StartTime,
                    dto.EndTime,
                    dto.ProjectName,
                    dto.Type,
                    participants,
                    mcPkgs,
                    commPkgs));
            return this.FromResult(result);
        }

        // TODO: Add permissions
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
                    new EditMeetingCommand(
                        dto.Meeting.Title,
                        dto.Meeting.BodyHtml,
                        dto.Meeting.Location,
                        dto.Meeting.StartTime,
                        dto.Meeting.EndTime,
                        dto.Meeting.RequiredParticipantOids,
                        dto.Meeting.RequiredParticipantEmails,
                        dto.Meeting.OptionalParticipantOids,
                        dto.Meeting.OptionalParticipantEmails)));
            return this.FromResult(result);
        }
    }
}
