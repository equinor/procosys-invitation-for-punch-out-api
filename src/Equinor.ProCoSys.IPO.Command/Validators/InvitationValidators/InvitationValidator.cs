using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Command.InvitationCommands.EditInvitation;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public class InvitationValidator : IInvitationValidator
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPlantProvider _plantProvider;
        private readonly IPersonApiService _personApiService;
        private readonly IPermissionCache _permissionCache;

        public InvitationValidator(IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider,
            IPersonApiService personApiService,
            IPlantProvider plantProvider,
            IPermissionCache permissionCache)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
            _personApiService = personApiService;
            _plantProvider = plantProvider;
            _permissionCache = permissionCache;
        }

        public async Task<bool> IpoExistsAsync(int invitationId, CancellationToken cancellationToken) =>
            await (from ipo in _context.QuerySet<Invitation>()
                   where ipo.Id == invitationId
                   select ipo).AnyAsync(cancellationToken);

        public async Task<bool> IpoIsInStageAsync(int invitationId, IpoStatus stage, CancellationToken cancellationToken) =>
            await (from ipo in _context.QuerySet<Invitation>()
                   where ipo.Id == invitationId && ipo.Status == stage
                   select ipo).AnyAsync(cancellationToken);

        public bool IsValidScope(
            DisciplineType type,
            IList<string> mcPkgScope,
            IList<string> commPkgScope)
        {
            switch (type)
            {
                case DisciplineType.DP:
                    return mcPkgScope.Any() && !commPkgScope.Any();
                case DisciplineType.MDP:
                    return !mcPkgScope.Any() && commPkgScope.Any();
                default:
                    return false;
            }
        }

        private bool IsValidExternalParticipant(ParticipantsForCommand participant)
        {
            var isValidEmail = new EmailAddressAttribute().IsValid(participant.InvitedExternalEmail.Email);
            return isValidEmail && participant.InvitedPerson == null && participant.InvitedFunctionalRole == null;
        }

        private bool IsValidPerson(IInvitedPersonForCommand invitedPerson)
        {
            if (invitedPerson.Email == null && (invitedPerson.AzureOid == Guid.Empty || invitedPerson.AzureOid == null))
            {
                return false;
            }

            return invitedPerson.AzureOid != Guid.Empty && invitedPerson.AzureOid != null ||
                   new EmailAddressAttribute().IsValid(invitedPerson.Email);
        }

        private bool IsValidPersonParticipant(ParticipantsForCommand participant)
            => IsValidPerson(participant.InvitedPerson) && participant.InvitedExternalEmail == null && participant.InvitedFunctionalRole == null;

        private bool IsValidFunctionalRoleParticipant(ParticipantsForCommand participant)
        {
            if (string.IsNullOrEmpty(participant.InvitedFunctionalRole.Code))
            {
                return false;
            }

            if (participant.InvitedFunctionalRole.InvitedPersons.Any(person => !IsValidPerson(person)))
            {
                return false;
            }

            return participant.InvitedPerson == null && participant.InvitedExternalEmail == null;
        }

        public bool IsValidParticipantList(IList<ParticipantsForCommand> participants)
        {
            foreach (var p in participants)
            {
                if (p.InvitedExternalEmail == null && p.InvitedPerson == null && p.InvitedFunctionalRole == null)
                {
                    return false;
                }
                if (p.Organization == Organization.External && !IsValidExternalParticipant(p))
                {
                    return false;
                }
                if (p.InvitedPerson != null && !IsValidPersonParticipant(p))
                {
                    return false;
                }
                if (p.InvitedFunctionalRole != null && !IsValidFunctionalRoleParticipant(p))
                {
                    return false;
                }
            }

            return true;
        }

        public bool RequiredParticipantsMustBeInvited(IList<ParticipantsForCommand> participants)
        {
            if (participants.Count < 2)
            {
                return false;
            }

            return participants.First().Organization == Organization.Contractor &&
                   participants.First().InvitedExternalEmail == null &&
                   participants[1].Organization == Organization.ConstructionCompany &&
                   participants[1].InvitedExternalEmail == null;
        }

        public bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
        {
            if (participants.Count < 2 || participants.First().SortKey != 0 || participants[1].SortKey != 1)
            {
                return false;
            }

            for (var i = 2; i < participants.Count; i++)
            {
                if (participants[i].SortKey == 0 || participants[i].SortKey == 1)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> AttachmentExistsAsync(int invitationId, int attachmentId, CancellationToken cancellationToken)
        {
            var invitation = await GetInvitationWithAttachments(invitationId, cancellationToken);
            return invitation?.Attachments.SingleOrDefault(a => a.Id == attachmentId) != null;
        }

        public async Task<bool> AttachmentWithFileNameExistsAsync(int invitationId, string fileName, CancellationToken cancellationToken)
        {
            var invitation = await GetInvitationWithAttachments(invitationId, cancellationToken);
            return invitation?.Attachments.SingleOrDefault(a => a.FileName.ToUpperInvariant() == fileName.ToUpperInvariant()) != null;
        }

        private async Task<Invitation> GetInvitationWithAttachments(int invitationId, CancellationToken cancellationToken)
        {
            var invitation = await (from i in _context.QuerySet<Invitation>().Include(i => i.Attachments)
                                    where i.Id == invitationId
                                    select i).SingleOrDefaultAsync(cancellationToken);
            return invitation;
        }

        public async Task<bool> ParticipantExistsAsync(int id, int invitationId, CancellationToken cancellationToken)
            => await (from p in _context.QuerySet<Participant>()
                      where p.Id == id && EF.Property<int>(p, "InvitationId") == invitationId
                      select p).AnyAsync(cancellationToken);

        public async Task<bool> ParticipantWithIdExistsAsync(ParticipantsForCommand participant, int invitationId, CancellationToken cancellationToken)
        {
            if (participant.InvitedPerson is InvitedPersonForEditCommand editPerson)
            {
                if (editPerson.Id.HasValue && !await ParticipantExistsAsync(editPerson.Id.Value, invitationId, cancellationToken))
                {
                    return false;
                }
            }

            if (participant.InvitedExternalEmail is InvitedExternalEmailForEditCommand externalEmail)
            {
                if (externalEmail.Id.HasValue && !await ParticipantExistsAsync(externalEmail.Id.Value, invitationId, cancellationToken))
                {
                    return false;
                }
            }

            if (participant.InvitedFunctionalRole is InvitedFunctionalRoleForEditCommand functionalRole)
            {
                if (functionalRole.Id.HasValue && !await ParticipantExistsAsync(functionalRole.Id.Value, invitationId, cancellationToken))
                {
                    return false;
                }

                foreach (var person in functionalRole.EditPersons)
                {
                    if (person.Id.HasValue && !await ParticipantExistsAsync(person.Id.Value, invitationId, cancellationToken))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public async Task<bool> ValidCompleterParticipantExistsAsync(int invitationId, CancellationToken cancellationToken)
        {
            var participants = await (from participant in _context.QuerySet<Participant>()
                                      where EF.Property<int>(participant, "InvitationId") == invitationId &&
                                            participant.SortKey == 0 &&
                                            participant.Organization == Organization.Contractor
                                      select participant).ToListAsync(cancellationToken);

            if (participants.Any(p => p.FunctionalRoleCode != null))
            {
                return participants
                    .SingleOrDefault(p => p.Type == IpoParticipantType.FunctionalRole) != null;
            }

            if (participants.SingleOrDefault() == null || participants.Single().Type != IpoParticipantType.Person)
            {
                return false;
            }
            return participants.Single().AzureOid == _currentUserProvider.GetCurrentUserOid();
        }

        public async Task<bool> ValidAccepterParticipantExistsAsync(int invitationId, CancellationToken cancellationToken)
        {
            var participants = await (from participant in _context.QuerySet<Participant>()
                                      where EF.Property<int>(participant, "InvitationId") == invitationId &&
                                            participant.SortKey == 1 &&
                                            participant.Organization == Organization.ConstructionCompany
                                      select participant).ToListAsync(cancellationToken);

            if (participants[0].FunctionalRoleCode != null)
            {
                return participants
                           .SingleOrDefault(p => p.SortKey == 1 &&
                                                 p.Type == IpoParticipantType.FunctionalRole) != null;
            }

            if (participants.Count != 1 || participants[0].Type != IpoParticipantType.Person)
            {
                return false;
            }
            return participants.First().AzureOid == _currentUserProvider.GetCurrentUserOid();
        }

        public async Task<bool> IpoHasCompleterAsync(int invitationId, CancellationToken cancellationToken) =>
            await (from participant in _context.QuerySet<Participant>()
                   where EF.Property<int>(participant, "InvitationId") == invitationId &&
                         participant.SortKey == 0 &&
                         participant.Organization == Organization.Contractor
                   select participant).AnyAsync(cancellationToken);

        public async Task<bool> IpoHasAccepterAsync(int invitationId, CancellationToken cancellationToken) =>
            await (from participant in _context.QuerySet<Participant>()
                   where EF.Property<int>(participant, "InvitationId") == invitationId &&
                         participant.SortKey == 1 &&
                         participant.Organization == Organization.ConstructionCompany
                   select participant).AnyAsync(cancellationToken);

        public async Task<bool> SignerExistsAsync(int invitationId, int participantId, CancellationToken cancellationToken) =>
            await (from participant in _context.QuerySet<Participant>()
                   where EF.Property<int>(participant, "InvitationId") == invitationId &&
                         participant.Id == participantId &&
                         (participant.Organization == Organization.TechnicalIntegrity ||
                          participant.Organization == Organization.Operation ||
                          participant.Organization == Organization.Commissioning ||
                          participant.Organization == Organization.Contractor ||
                          participant.Organization == Organization.ConstructionCompany)
                   select participant).AnyAsync(cancellationToken);

        public async Task<bool> ValidSigningParticipantExistsAsync(int invitationId, int participantId, CancellationToken cancellationToken)
        {
            var participant = await (from p in _context.QuerySet<Participant>()
                                     where EF.Property<int>(p, "InvitationId") == invitationId &&
                                           p.Id == participantId &&
                                           p.SortKey > 1 &&
                                           (p.Organization == Organization.TechnicalIntegrity ||
                                            p.Organization == Organization.Operation ||
                                            p.Organization == Organization.Commissioning ||
                                            p.Organization == Organization.Contractor ||
                                            p.Organization == Organization.ConstructionCompany)
                                     select p).SingleAsync(cancellationToken);

            if (participant.Type == IpoParticipantType.FunctionalRole)
            {
                return true;
            }

            return participant.AzureOid == _currentUserProvider.GetCurrentUserOid();
        }

        public async Task<bool> SameUserUnCompletingThatCompletedAsync(int invitationId, CancellationToken cancellationToken)
        {
            var completingPerson = await (from i in _context.QuerySet<Invitation>()
                                          join p in _context.QuerySet<Person>() on i.CompletedBy equals p.Id
                                          where i.Id == invitationId
                                          select p).SingleOrDefaultAsync(cancellationToken);

            return completingPerson != null && _currentUserProvider.GetCurrentUserOid() == completingPerson.Oid;
        }

        public async Task<bool> SameUserUnAcceptingThatAcceptedAsync(int invitationId, CancellationToken cancellationToken)
        {
            var acceptingPerson = await (from i in _context.QuerySet<Invitation>()
                                         join p in _context.QuerySet<Person>() on i.AcceptedBy equals p.Id
                                         where i.Id == invitationId
                                         select p).SingleOrDefaultAsync(cancellationToken);

            return acceptingPerson != null && _currentUserProvider.GetCurrentUserOid() == acceptingPerson.Oid;
        }

        public async Task<bool> CurrentUserIsCreatorOrIsInContractorFunctionalRoleOfInvitationAsync(int invitationId, CancellationToken cancellationToken)
        {
            var currentUserIsCreator = CurrentUserIsCreatorOfIpoAsync(invitationId, cancellationToken).Result;
            if (currentUserIsCreator) return true;
            // allow if user is member of contractor functional role
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var contractorParticipants = await (from participant in _context.QuerySet<Participant>()
                where EF.Property<int>(participant, "InvitationId") == invitationId &&
                      participant.Organization == Organization.Contractor
                select participant).ToListAsync(cancellationToken);

            if (contractorParticipants.Any(p => p.FunctionalRoleCode != null))
            {
                var contractorCode = contractorParticipants.First().FunctionalRoleCode;
                var person = await _personApiService.GetPersonInFunctionalRoleAsync(
                    _plantProvider.Plant,
                    currentUserOid.ToString(),
                    contractorCode);

                return person != null && new Guid(person.AzureOid) == currentUserOid;
            }
            return false;
        }

        public async Task<bool> CurrentUserIsCreatorOfInvitationOrAdminAsync(int invitationId,
            CancellationToken cancellationToken)
        {
            var currentUserIsCreator = CurrentUserIsCreatorOfIpoAsync(invitationId, cancellationToken).Result;
            return currentUserIsCreator || await InvitationHelper.HasIpoAdminPrivilege(_permissionCache, _plantProvider, _currentUserProvider);
        }

        private async Task<bool> CurrentUserIsCreatorOfIpoAsync(int invitationId, CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();

            var currentUserId = await (from person in _context.QuerySet<Person>()
                    where person.Oid == currentUserOid
                    select person.Id)
                .SingleAsync(cancellationToken);

            var createdById = await (from invitation in _context.QuerySet<Invitation>()
                    where invitation.Id == invitationId
                    select invitation.CreatedById)
                .SingleAsync(cancellationToken);

            return currentUserId == createdById;
        }
    }
}
