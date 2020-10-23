using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Query.GetAttachments;
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
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
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
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
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
                            ? new PersonForCommand(p.Person.AzureOid, p.Person.FirstName, p.Person.LastName, p.Person.Email,
                                p.Person.Required)
                            : null,
                        p.FunctionalRole != null 
                            ? new FunctionalRoleForCommand(
                                p.FunctionalRole.Code,
                                p.FunctionalRole.Email,
                                p.FunctionalRole.UsePersonalEmail,
                                p.FunctionalRole.Persons?.Select(person =>
                                    new PersonForCommand(
                                        person.AzureOid,
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
                    dto.Description,
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
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
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

        // TODO: Add permissions
        [HttpPost("{id}/Attachments")]
        public async Task<ActionResult<int>> UploadAttachment(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromForm] UploadAttachmentDto dto)
        {
            await using var stream = dto.File.OpenReadStream();

            var command = new UploadAttachmentCommand(
                id,
                dto.File.FileName,
                dto.OverwriteIfExists,
                stream);

            var result = await _mediator.Send(command);
            return this.FromResult(result);
        }

        // TODO: Add permissions
        [HttpGet("{id}/Attachments/{attachmentId}")]
        public async Task<ActionResult<int>> GetAttachment(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromRoute] int attachmentId)
        {
            var result = await _mediator.Send(new GetAttachmentByIdQuery(id, attachmentId));
            return this.FromResult(result);
        }

        // TODO: Add permissions
        [HttpGet("{id}/Attachments")]
        public async Task<ActionResult<List<AttachmentDto>>> GetAttachments(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id)
        {
            var result = await _mediator.Send(new GetAttachmentsQuery(id));
            return this.FromResult(result);
        }

        // TODO: Add permissions
        [HttpDelete("{id}/Attachments/{attachmentId}")]
        public async Task<ActionResult<int>> DeleteAttachment(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromRoute] int attachmentId,
            [FromBody] DeleteAttachmentDto dto)
        {
            var command = new DeleteAttachmentCommand(
                id,
                attachmentId,
                dto.RowVersion);

            var result = await _mediator.Send(command);
            return this.FromResult(result);
        }
    }
}
