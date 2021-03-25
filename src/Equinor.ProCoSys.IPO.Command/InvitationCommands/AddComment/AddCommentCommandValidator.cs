using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using FluentValidation;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.AddComment
{
    public class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator(IInvitationValidator invitationValidator)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(command => command)
                //input validators
                .Must((command) => command.Comment.Length <= Comment.CommentMaxLength)
                .WithMessage(command =>
                    $"Comment cannot be more than {Comment.CommentMaxLength} characters! Comment={command.Comment}")
                //business validators
                .MustAsync((command, cancellationToken) => BeAnExistingInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    $"IPO with this ID does not exist! Id={command.InvitationId}")
                .MustAsync((command, cancellationToken) => BeANonCanceledInvitation(command.InvitationId, cancellationToken))
                .WithMessage(command =>
                    "Invitation is canceled, and thus cannot be commented on!");

            async Task<bool> BeAnExistingInvitation(int invitationId, CancellationToken cancellationToken)
                => await invitationValidator.IpoExistsAsync(invitationId, cancellationToken);

            async Task<bool> BeANonCanceledInvitation(int invitationId, CancellationToken cancellationToken)
                => !await invitationValidator.IpoIsInStageAsync(invitationId, IpoStatus.Canceled, cancellationToken);
        }
    }
}
