using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CancelPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CreateInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.DeleteAttachment;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.CompletePunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.SignPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UnAcceptPunchOut;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UpdateAttendedStatusAndNotesOnParticipants;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.UploadAttachment;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query.GetAttachmentById;
using Equinor.ProCoSys.IPO.Query.GetAttachments;
using Equinor.ProCoSys.IPO.Query.GetHistory;
using Equinor.ProCoSys.IPO.Query.GetComments;
using Equinor.ProCoSys.IPO.Query.GetInvitationById;
using Equinor.ProCoSys.IPO.Query.GetInvitations;
using Equinor.ProCoSys.IPO.Query.GetInvitationsByCommPkgNo;
using Equinor.ProCoSys.IPO.Query.GetLatestMdpIpoStatusOnCommPkgs;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;
using InvitationDto = Equinor.ProCoSys.IPO.Query.GetInvitationById.InvitationDto;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    [ApiController]
    [Route("Invitations")]
    public class InvitationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public InvitationsController(IMediator mediator) => _mediator = mediator;

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet]
        public async Task<ActionResult<InvitationsResult>> GetTags(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            string plant,
            [FromQuery] FilterDto filter,
            [FromQuery] SortingDto sorting,
            [FromQuery] PagingDto paging)
        {
            var query = CreateGetInvitationsQuery(filter, sorting, paging);

            var result = await _mediator.Send(query);
            return this.FromResult(result);
        }

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
        [HttpGet("ByCommPkgNo/{commPkgNo}")]
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

        [Authorize(Roles = Permissions.COMMPKG_READ)]
        [HttpGet("ByCommPkgNos")]
        public async Task<ActionResult<CommPkgsWithMdpIposDto>> GetInvitationsByCommPkgNos(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromQuery] IList<string> commPkgNos,
            [FromQuery] string projectName)
        {
            var result = await _mediator.Send(new GetLatestMdpIpoStatusOnCommPkgsQuery(commPkgNos, projectName));
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
                    dto.Type,
                    updatedParticipants,
                    dto.UpdatedMcPkgScope,
                    dto.UpdatedCommPkgScope,
                    dto.RowVersion));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/Cancel")]
        public async Task<ActionResult> CancelInvitation(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] CancelPunchOutDto dto)
        {
            var result = await _mediator.Send(
                new CancelPunchOutCommand(id, dto.RowVersion));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [HttpPut("{id}/Sign")]
        public async Task<ActionResult<string>> SignPunchOut(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] SignPunchOutDto dto)
        {
            var result = await _mediator.Send(
                new SignPunchOutCommand(id, dto.ParticipantId, dto.ParticipantRowVersion));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/Accept")]
        public async Task<ActionResult<string>> AcceptPunchOut(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] AcceptPunchOutDto dto)
        {
            var participants = dto.Participants?.Select(p =>
                new UpdateNoteOnParticipantForCommand(p.Id, p.Note, p.RowVersion)).ToList();
            var result = await _mediator.Send(
                new AcceptPunchOutCommand(id, dto.InvitationRowVersion, dto.ParticipantRowVersion, participants));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_SIGN)]
        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPut("{id}/Unaccept")]
        public async Task<ActionResult<string>> UnacceptPunchOut(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] UnAcceptPunchOutDto dto)
        {
            var result = await _mediator.Send(
                new UnAcceptPunchOutCommand(id, dto.InvitationRowVersion, dto.ParticipantRowVersion));
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
            var participantsToUpdate = dto.Participants?.Select(p =>
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

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("{id}/History")]
        public async Task<ActionResult<HistoryDto>> GetHistory(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id)
        {
            var result = await _mediator.Send(new GetHistoryQuery(id));
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

        [Authorize(Roles = Permissions.IPO_WRITE)]
        [HttpPost("{id}/Comments")]
        public async Task<ActionResult<int>> AddComment(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id,
            [FromBody] AddCommentDto dto)
        {
            var result = await _mediator.Send(
                new AddCommentCommand(id, dto.Comment));
            return this.FromResult(result);
        }

        [Authorize(Roles = Permissions.IPO_READ)]
        [HttpGet("{id}/Comments")]
        public async Task<ActionResult<List<CommentDto>>> GetComments(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
            [Required]
            [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
            string plant,
            [FromRoute] int id)
        {
            var result = await _mediator.Send(new GetCommentsQuery(id));
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
                                    person.Email,
                                    person.Required,
                                    person.Id,
                                    person.RowVersion)).ToList(),
                            p.FunctionalRole.Id,
                            p.FunctionalRole.RowVersion)
                        : null,
                    p.SortKey)
            ).ToList();

        private static GetInvitationsQuery CreateGetInvitationsQuery(FilterDto filter, SortingDto sorting, PagingDto paging)
        {
            var query = new GetInvitationsQuery(
                filter.ProjectName,
                new Sorting(sorting.Direction, sorting.Property),
                new Filter(),
                new Paging(paging.Page, paging.Size)
            );

            FillFilterFromDto(filter, query.Filter);

            return query;
        }

        private static void FillFilterFromDto(FilterDto source, Filter target)
        {
            if (source.PunchOutDates != null)
            {
                target.PunchOutDates = source.PunchOutDates.ToList();
            }

            if (source.IpoStatuses != null)
            {
                target.IpoStatuses = source.IpoStatuses.ToList();
            }

            if (source.FunctionalRoleCode != null)
            {
                target.FunctionalRoleCode = source.FunctionalRoleCode;
            }

            if (source.PersonOid != null)
            {
                target.PersonOid = source.PersonOid;
            }

            if (source.CommPkgNoStartsWith != null)
            {
                target.CommPkgNoStartsWith = source.CommPkgNoStartsWith;
            }

            if (source.McPkgNoStartsWith != null)
            {
                target.McPkgNoStartsWith = source.McPkgNoStartsWith;
            }

            if (source.TitleStartsWith != null)
            {
                target.TitleStartsWith = source.TitleStartsWith;
            }

            if (source.IpoIdStartsWith != null)
            {
                target.IpoIdStartsWith = source.IpoIdStartsWith;
            }

            if (source.PunchOutDateFromUtc != null)
            {
                target.PunchOutDateFromUtc = source.PunchOutDateFromUtc;
            }

            if (source.PunchOutDateToUtc != null)
            {
                target.PunchOutDateToUtc = source.PunchOutDateToUtc;
            }

            if (source.LastChangedAtFromUtc != null)
            {
                target.LastChangedAtFromUtc = source.LastChangedAtFromUtc;
            }

            if (source.LastChangedAtToUtc != null)
            {
                target.LastChangedAtToUtc = source.LastChangedAtToUtc;
            }
        }
    }
}
