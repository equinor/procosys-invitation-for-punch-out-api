using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command.InvitationCommands;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Command.Validators.InvitationValidators
{
    public class InvitationValidator : IInvitationValidator
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;

        public InvitationValidator(IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<bool> IpoExistsAsync(int invitationId, CancellationToken token) =>
            await (from ipo in _context.QuerySet<Invitation>()
                where ipo.Id == invitationId
                select ipo).AnyAsync(token);

        public async Task<bool> IpoIsInStageAsync(int invitationId, IpoStatus stage, CancellationToken token) => 
            await(from ipo in _context.QuerySet<Invitation>()
                    where ipo.Id == invitationId && ipo.Status == stage
                    select ipo).AnyAsync(token);

        public bool IsValidScope(
            IList<string> mcPkgScope,
            IList<string> commPkgScope) 
                => (mcPkgScope.Count > 0 || commPkgScope.Count > 0) && (mcPkgScope.Count < 1 || commPkgScope.Count < 1);

        public async Task<bool> IpoTitleExistsInProjectAsync(string projectName, string title, CancellationToken token)
        => await(from invitation in _context.QuerySet<Invitation>()
                where invitation.Title == title && invitation.ProjectName == projectName
                select invitation).AnyAsync(token);

        public async Task<bool> IpoTitleExistsInProjectOnAnotherIpoAsync(string projectName, string title, int id, CancellationToken token)
            => await (from invitation in _context.QuerySet<Invitation>()
                where invitation.Title == title && 
                      invitation.ProjectName == projectName &&
                      invitation.Id != id
                select invitation).AnyAsync(token);

        private bool IsValidExternalParticipant(ParticipantsForCommand participant)
        { 
            var isValidEmail = new EmailAddressAttribute().IsValid(participant.ExternalEmail.Email);
            return isValidEmail && participant.Person == null && participant.FunctionalRole == null;
        }

        private bool IsValidPerson(ParticipantsForCommand participant)
        {
            if (participant.Person.Email == null && (participant.Person.AzureOid == Guid.Empty || participant.Person.AzureOid == null))
            {
                return false;
            }
            var isValidEmail = new EmailAddressAttribute().IsValid(participant.Person.Email);
            return participant.ExternalEmail == null && participant.FunctionalRole == null && isValidEmail;
        }

        private bool IsValidFunctionalRole(ParticipantsForCommand participant)
        {
            foreach (var person in participant.FunctionalRole.Persons)
            {
                if (!(new EmailAddressAttribute().IsValid(person.Email)))
                {
                    return false;
                }
            }
            
            return participant.Person == null && participant.ExternalEmail == null;
        }

        public bool IsValidParticipantList(IList<ParticipantsForCommand> participants)
        {
            foreach (var p in participants)
            {
                if (p.ExternalEmail == null && p.Person == null && p.FunctionalRole == null)
                {
                    return false;
                }
                if (p.Organization == Organization.External && !IsValidExternalParticipant(p))
                {
                    return false;
                }
                if (p.Person != null && !IsValidPerson(p))
                {
                    return false;
                }
                if (p.FunctionalRole != null && !IsValidFunctionalRole(p))
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

            return participants[0].Organization == Organization.Contractor &&
                   participants[0].ExternalEmail == null &&
                   participants[1].Organization == Organization.ConstructionCompany &&
                   participants[1].ExternalEmail == null;
        }

        public bool OnlyRequiredParticipantsHaveLowestSortKeys(IList<ParticipantsForCommand> participants)
        {
            if (participants.Count < 2 || participants[0].SortKey != 0 || participants[1].SortKey != 1)
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

        public async Task<bool> ParticipantExists(int? id, int invitationId, CancellationToken token) 
            => await(from p in _context.QuerySet<Participant>()
                where p.Id == id && EF.Property<int>(p, "InvitationId") == invitationId
                     select p).AnyAsync(token);

        public async Task<bool> ParticipantWithIdExistsAsync(ParticipantsForCommand participant, int invitationId, CancellationToken token)
        {
            if (participant.Person?.Id != null && !await ParticipantExists(participant.Person.Id, invitationId, token))
            {
                return false;
            }
            if (participant.ExternalEmail?.Id != null && !await ParticipantExists(participant.ExternalEmail.Id, invitationId, token))
            {
                return false;
            }
            if (participant.FunctionalRole != null)
            {
                if (!await ParticipantExists(participant.FunctionalRole.Id, invitationId, token))
                {
                    return false;
                }

                foreach (var person in participant.FunctionalRole.Persons)
                {
                    if (person.Id != null && !await ParticipantExists(person.Id, invitationId, token))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public async Task<bool> ProjectNameIsNotChangedAsync(string projectName, int id, CancellationToken token) 
            => await (from invitation in _context.QuerySet<Invitation>() 
                where invitation.Id == id && invitation.ProjectName == projectName
                select invitation).AnyAsync(token);

        public async Task<bool> ValidContractorParticipantExistsAsync(int invitationId, CancellationToken token)
        {
            var participants = await(from participant in _context.QuerySet<Participant>()
                where EF.Property<int>(participant, "InvitationId") == invitationId &&
                      participant.SortKey == 0 &&
                      participant.Organization == Organization.Contractor
                select participant).ToListAsync(token);

            if (participants[0].FunctionalRoleCode != null)
            {
                return participants
                    .SingleOrDefault(p => p.SortKey == 0 &&
                                          p.Type == IpoParticipantType.FunctionalRole) != null;
            }

            if (participants.Count != 1 || participants[0].Type != IpoParticipantType.Person)
            {
                return false;
            }
            return participants.First().AzureOid == _currentUserProvider.GetCurrentUserOid();
        }

        public async Task<bool> ValidConstructionCompanyParticipantExistsAsync(int invitationId, CancellationToken token)
        {
            var participants = await (from participant in _context.QuerySet<Participant>()
                where EF.Property<int>(participant, "InvitationId") == invitationId &&
                      participant.SortKey == 1 &&
                      participant.Organization == Organization.ConstructionCompany
                select participant).ToListAsync(token);

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

        public async Task<bool> ContractorExistsAsync(int invitationId, CancellationToken token) =>
            await (from participant in _context.QuerySet<Participant>()
                where EF.Property<int>(participant, "InvitationId") == invitationId &&
                      participant.SortKey == 0 &&
                      participant.Organization == Organization.Contractor
                select participant).AnyAsync(token);

        public async Task<bool> ValidConstructionCompanyExistsAsync(int invitationId, CancellationToken token) =>
            await (from participant in _context.QuerySet<Participant>()
                where EF.Property<int>(participant, "InvitationId") == invitationId &&
                      participant.SortKey == 1 &&
                      participant.Organization == Organization.ConstructionCompany
                select participant).AnyAsync(token);
    }
}
