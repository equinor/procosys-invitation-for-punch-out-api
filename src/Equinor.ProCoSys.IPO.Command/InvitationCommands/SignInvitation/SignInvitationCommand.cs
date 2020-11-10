using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.SignInvitation
{
    public class SignInvitationCommand : IRequest<Result<string>>
    {
        public SignInvitationCommand(
            int invitationId,
            IList<ParticipantsWhenSigningForCommand> participants)
        {
            InvitationId = invitationId;
            Participants = participants ?? new List<ParticipantsWhenSigningForCommand>();
        }
        public int InvitationId { get; set; }
        public IList<ParticipantsWhenSigningForCommand> Participants { get; }
    }
}
