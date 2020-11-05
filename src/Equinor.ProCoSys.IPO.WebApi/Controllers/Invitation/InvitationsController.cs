using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
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
            var participants = GetParticipantsForCommands(dto.Participants);

            var result = await _mediator.Send(
                new CreateInvitationCommand(
                    dto.Title,
                    dto.Description,
                    dto.Location,
                    dto.StartTime,
                    dto.EndTime,
                    dto.ProjectName,
                    dto.Type,
                    participants,
                    dto.McPkgScope,
                    dto.CommPkgScope));
            return this.FromResult(result);
        }

        // TODO: Add permissions
        [HttpPut("{id}")]
        public async Task<IActionResult> EditInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromRoute] int id,
            [FromBody] EditInvitationDto dto)
        {
            var updatedParticipants = GetParticipantsForCommands(dto.UpdatedParticipants);

            var result = await _mediator.Send(
                new EditInvitationCommand(
                    id,
                    dto.Title,
                    dto.Description,
                    dto.Location,
                    dto.StartTime,
                    dto.EndTime,
                    dto.ProjectName,
                    dto.Type,
                    updatedParticipants,
                    dto.UpdatedMcPkgScope,
                    dto.UpdatedCommPkgScope,
                    dto.RowVersion));
            return this.FromResult(result);
        }

        private IList<ParticipantsForCommand> GetParticipantsForCommands(IEnumerable<ParticipantDto> dto)
            => dto?.Select(p =>
                new ParticipantsForCommand(
                    p.Organization,
                    p.ExternalEmail != null
                        ? new ExternalEmailForCommand(
                            p.ExternalEmail.Email,
                            p.ExternalEmail.Id,
                            p.ExternalEmail.RowVersion)
                    : null,
                    p.Person != null
                        ? new PersonForCommand(
                            p.Person.AzureOid,
                            p.Person.FirstName,
                            p.Person.LastName,
                            p.Person.Email,
                            p.Person.Required,
                            p.Person.Id,
                            p.Person.RowVersion)
                        : null,
                    p.FunctionalRole != null
                        ? new FunctionalRoleForCommand(
                            p.FunctionalRole.Code,
                            p.FunctionalRole.Persons?.Select(person =>
                                new PersonForCommand(
                                    person.AzureOid,
                                    p.Person.FirstName,
                                    p.Person.LastName,
                                    person.Email,
                                    person.Required,
                                    person.Id,
                                    person.RowVersion)).ToList(),
                            p.FunctionalRole.Id,
                            p.FunctionalRole.RowVersion)
                        : null,
                    p.SortKey)
            ).ToList();
    }
}
