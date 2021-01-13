using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public interface IInvitationValidator
    {
        Task<bool> IpoTitleExistsInProjectAsync(string projectName, string title, CancellationToken token);
        Task<bool> IpoTitleExistsInProjectOnAnotherIpoAsync(string title, int id, CancellationToken token);
        bool IsValidScope(IList<string> mcPkgScope, IList<string> commPkgScope);
        Task<bool> ParticipantWithIdExistsAsync(ParticipantsForCommand participant, int invitationId, CancellationToken token);
        Task<bool> ParticipantExistsAsync(int? id, int invitationId, CancellationToken token);
        bool IsValidParticipantList(IList<ParticipantsForCommand> participants);
        bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants);
        bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants);
        Task<bool> AttachmentExistsAsync(int invitationId, int attachmentId, CancellationToken cancellationToken);
        Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken);
        Task<bool> IpoExistsAsync(int invitationId, CancellationToken token);
        Task<bool> IpoIsInStageAsync(int invitationId, IpoStatus stage, CancellationToken token);
        Task<bool> ValidContractorParticipantExistsAsync(int invitationId, CancellationToken token);
        Task<bool> ValidConstructionCompanyParticipantExistsAsync(int invitationId, CancellationToken token);
        Task<bool> ContractorExistsAsync(int invitationId, CancellationToken token);
        Task<bool> ConstructionCompanyExistsAsync(int invitationId, CancellationToken token);
        Task<bool> SignerExistsAsync(int invitationId, int participantId, CancellationToken token);
        Task<bool> ValidSigningParticipantExistsAsync(int invitationId, int participantId, CancellationToken token);
        Task<bool> CurrentUserIsCreatorOfInvitation(int invitationId, CancellationToken token);
        Task<bool> SameUserUnAcceptingThatAcceptedAsync(int invitationId, CancellationToken token);
    }
}
