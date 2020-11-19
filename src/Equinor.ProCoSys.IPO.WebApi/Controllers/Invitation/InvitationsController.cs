﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Query.GetAttachments;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("{id}")]
        public async Task<ActionResult<InvitationDto>> GetInvitationById(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id)
        {
            var result = await _mediator.Send(new GetInvitationByIdQuery(id));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.COMMPKG_READ)]
        [HttpGet("/ByCommPkgNo/{commPkgNo}")]
        public async Task<ActionResult<InvitationForMainDto>> GetInvitationsByCommPkgNo(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] string commPkgNo,
            [FromQuery] string projectName)
        {
            var result = await _mediator.Send(new GetInvitationsByCommPkgNoQuery(commPkgNo, projectName));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_CREATE)]
        [HttpPost]
        public async Task<ActionResult<int>> CreateInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
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

        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}")]
        public async Task<ActionResult<string>> EditInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
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

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/Accept")]
        public async Task<ActionResult> AcceptPunchOut(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] AcceptPunchOutDto dto)
        {
            var participants = dto.Participants.Select(p =>
                new UpdateNoteOnParticipantForCommand(p.Id, p.Note, p.RowVersion)).ToList();
            var result = await _mediator.Send(
                new AcceptPunchOutCommand(id, dto.InvitationRowVersion, dto.ParticipantRowVersion, participants));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/Complete")]
        public async Task<ActionResult<string>> CompletePunchOut(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] CompletePunchOutDto dto)
        {
            var participantsToUpdate = dto.Participants.Select(p =>
                new UpdateAttendedStatusAndNoteOnParticipantForCommand(p.Id, p.Attended, p.Note, p.RowVersion));
            var result = await _mediator.Send(
                new CompletePunchOutCommand(id, dto.InvitationRowVersion, dto.ParticipantRowVersion, participantsToUpdate));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/AttendedStatusAndNotes")]
        public async Task<ActionResult> ChangeAttendedStatusOnParticipants(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] ParticipantToChangeDto[] dto)
        {
            var participants = dto.Select(p =>
                new UpdateAttendedStatusAndNoteOnParticipantForCommand(p.Id, p.Attended, p.Note, p.RowVersion)).ToList();
            var result = await _mediator.Send(
                new UpdateAttendedStatusAndNotesOnParticipantsCommand(id, participants));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_ATTACHFILE)]
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

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("{id}/Attachments/{attachmentId}")]
        public async Task<ActionResult<AttachmentDto>> GetAttachment(
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

        [Authorize(Roles = Permissions.IPO_READ)]
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

        [Authorize(Roles = Permissions.IPO_DETACHFILE)]
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
