using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Command.EventPublishers;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.ForeignApi;
using Equinor.ProCoSys.IPO.ForeignApi.LibraryApi.FunctionalRole;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Person;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.EditParticipants
{
    public class EditParticipantsCommandHandler : IRequestHandler<EditParticipantsCommand, Result<Unit>>
    {
        private const string _objectName = "IPO";
        private readonly IList<string> _signerPrivileges = new List<string> { "SIGN" };

        private readonly IInvitationRepository _invitationRepository;
        private readonly IPlantProvider _plantProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPersonApiService _personApiService;
        private readonly IFunctionalRoleApiService _functionalRoleApiService;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public EditParticipantsCommandHandler(
            IInvitationRepository invitationRepository, 
            IPlantProvider plantProvider,
            IUnitOfWork unitOfWork,
            IPersonApiService personApiService,
            IFunctionalRoleApiService functionalRoleApiService,
            IIntegrationEventPublisher integrationEventPublisher)
        {
            _invitationRepository = invitationRepository;
            _plantProvider = plantProvider;
            _unitOfWork = unitOfWork;
            _personApiService = personApiService;
            _functionalRoleApiService = functionalRoleApiService;
            _integrationEventPublisher = integrationEventPublisher;
        }

        public async Task<Result<Unit>> Handle(EditParticipantsCommand request, CancellationToken cancellationToken)
        {
            var invitation = await _invitationRepository.GetByIdAsync(request.InvitationId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                await UpdateParticipants(request.UpdatedParticipants, invitation);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                //TODO: JSOI Revert this back to original state

               // await PublishEventToBusAsync(cancellationToken, invitation);

                //await _unitOfWork.SaveChangesAsync(cancellationToken);

                //TODO: Remember to test with functional roles, we don't want to send one message per member in a functional role
                _unitOfWork.Commit();

            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            return new SuccessResult<Unit>(Unit.Value);
        }

        //private async Task PublishEventToBusAsync(CancellationToken cancellationToken, Invitation invitation)
        //{
        //    foreach (var participant in invitation.Participants)
        //    {
        //        var participantMessage = _invitationRepository.GetParticipantEvent(invitation.Id, participant.Id);
        //        await _integrationEventPublisher.PublishAsync(participantMessage, cancellationToken);
        //    }
        //}

        private async Task UpdateParticipants(
            IList<ParticipantsForEditCommand> participantsToUpdate,
            Invitation invitation)
        {
            var existingParticipants = invitation.Participants.ToList();

            var functionalRoleParticipants =
                participantsToUpdate.Where(p => p.InvitedFunctionalRoleToEdit != null).ToList();
            var functionalRoleParticipantIds = functionalRoleParticipants.Select(p => p.InvitedFunctionalRoleToEdit.Id).ToList();

            var persons = participantsToUpdate.Where(p => p.InvitedPersonToEdit != null).ToList();
            var personsIds = persons.Select(p => p.InvitedPersonToEdit.Id).ToList();

            var externalEmailParticipants = participantsToUpdate.Where(p => p.InvitedExternalEmailToEdit != null).ToList();
            var externalEmailParticipantsIds = externalEmailParticipants.Select(p => p.InvitedExternalEmailToEdit.Id).ToList();

            var participantsToUpdateIds = externalEmailParticipantsIds
                .Concat(personsIds)
                .Concat(functionalRoleParticipantIds).ToList();
            participantsToUpdateIds.AddRange(from fr in functionalRoleParticipants where fr.InvitedPersonToEdit != null select fr.InvitedPersonToEdit.Id);
            foreach (var functionalRoleParticipant in functionalRoleParticipants)
            {
                participantsToUpdateIds.AddRange(functionalRoleParticipant.InvitedFunctionalRoleToEdit.EditPersons.Select(person => person.Id));
            }

            var participantsToDelete = existingParticipants.Where(p => !participantsToUpdateIds.Contains(p.Id));
            foreach (var participantToDelete in participantsToDelete)
            {
                invitation.RemoveParticipant(participantToDelete);
                _invitationRepository.RemoveParticipant(participantToDelete);
                //TODO: Add publish delete here?
            }

            if (functionalRoleParticipants.Count > 0)
            {
                await UpdateFunctionalRoleParticipantsAsync(invitation, functionalRoleParticipants, existingParticipants);
            }
            if (persons.Count > 0)
            {
                await AddPersonParticipantsWithOidsAsync(invitation, persons, existingParticipants);
            }
            
            AddExternalParticipant(invitation, externalEmailParticipants, existingParticipants);
        }

        private async Task UpdateFunctionalRoleParticipantsAsync(
            Invitation invitation,
            IList<ParticipantsForEditCommand> functionalRoleParticipants,
            IList<Participant> existingParticipants)
        {
            var codes = functionalRoleParticipants.Select(p => p.InvitedFunctionalRoleToEdit.Code).ToList();
            var functionalRoles =
                await _functionalRoleApiService.GetFunctionalRolesByCodeAsync(_plantProvider.Plant, codes);

            foreach (var participant in functionalRoleParticipants)
            {
                var fr = functionalRoles.SingleOrDefault(p => p.Code == participant.InvitedFunctionalRole.Code);
                if (fr != null)
                {
                    var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedFunctionalRoleToEdit.Id);
                    if (existingParticipant != null)
                    {
                        invitation.UpdateParticipant(
                            existingParticipant.Id,
                            participant.Organization,
                            IpoParticipantType.FunctionalRole,
                            fr.Code,
                            null,
                            null,
                            fr.Email,
                            null,
                            participant.SortKey,
                            participant.InvitedFunctionalRoleToEdit.RowVersion);
                    }
                    else
                    {
                        invitation.AddParticipant(new Participant(
                            _plantProvider.Plant,
                            participant.Organization,
                            IpoParticipantType.FunctionalRole,
                            fr.Code,
                            null,
                            null,
                            null,
                            fr.Email,
                            null,
                            participant.SortKey));
                    }
                    
                    foreach (var person in participant.InvitedFunctionalRoleToEdit.EditPersons)
                    {
                        var frPerson = fr.Persons.SingleOrDefault(p => p.AzureOid == person.AzureOid.ToString());
                        if (frPerson != null)
                        {
                            var existingPerson = existingParticipants.SingleOrDefault(p => p.Id == person.Id);
                            if (existingPerson != null)
                            {
                                invitation.UpdateParticipant(
                                    existingPerson.Id,
                                    participant.Organization,
                                    IpoParticipantType.Person,
                                    participant.InvitedFunctionalRole.Code,
                                    frPerson.FirstName,
                                    frPerson.LastName,
                                    frPerson.Email,
                                    new Guid(frPerson.AzureOid),
                                    participant.SortKey,
                                    person.RowVersion);
                            }
                            else
                            {
                                invitation.AddParticipant(new Participant(
                                    _plantProvider.Plant,
                                    participant.Organization,
                                    IpoParticipantType.Person,
                                    fr.Code,
                                    frPerson.FirstName,
                                    frPerson.LastName,
                                    frPerson.UserName,
                                    frPerson.Email,
                                    new Guid(frPerson.AzureOid),
                                    participant.SortKey));
                            }
                        }
                    }
                }
                else
                {
                    throw new IpoValidationException(
                        $"Could not find functional role with functional role code '{participant.InvitedFunctionalRole.Code}' on participant {participant.Organization}.");
                }
            }
        }

        private async Task AddPersonParticipantsWithOidsAsync(
            Invitation invitation,
            List<ParticipantsForEditCommand> personParticipantsWithOids,
            IList<Participant> existingParticipants)
        {
            var personsAdded = new List<ParticipantsForCommand>();

            foreach (var participant in personParticipantsWithOids)
            {
                if (InvitationHelper.ParticipantIsSigningParticipant(participant))
                {
                    await AddSigner(
                        invitation,
                        existingParticipants,
                        participant.InvitedPersonToEdit,
                        participant.SortKey,
                        participant.Organization);
                    personsAdded.Add(participant);
                }
            }

            personParticipantsWithOids.RemoveAll(p => personsAdded.Contains(p));

            var oids = personParticipantsWithOids.Where(p => p.SortKey > 1)
                .Select(p => p.InvitedPersonToEdit.AzureOid.ToString())
                .ToList();
            var persons = oids.Count > 0
                ? await _personApiService.GetPersonsByOidsAsync(_plantProvider.Plant, oids)
                : new List<ProCoSysPerson>();
            if (persons.Any())
            {
                foreach (var participant in personParticipantsWithOids)
                {
                    var person = persons.SingleOrDefault(p => p.AzureOid == participant.InvitedPersonToEdit.AzureOid.ToString());
                    if (person != null)
                    {
                        var existingParticipant =
                            existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedPersonToEdit.Id);
                        if (existingParticipant != null)
                        {
                            invitation.UpdateParticipant(
                                existingParticipant.Id,
                                participant.Organization,
                                IpoParticipantType.Person,
                                null,
                                person.FirstName,
                                person.LastName,
                                person.Email,
                                new Guid(person.AzureOid),
                                participant.SortKey,
                                participant.InvitedPersonToEdit.RowVersion);
                        }
                        else
                        {
                            invitation.AddParticipant(new Participant(
                                _plantProvider.Plant,
                                participant.Organization,
                                IpoParticipantType.Person,
                                null,
                                person.FirstName,
                                person.LastName,
                                person.UserName,
                                person.Email,
                                new Guid(person.AzureOid),
                                participant.SortKey));
                        }
                    }
                }
            }
        }

        private async Task AddSigner(
            Invitation invitation,
            IList<Participant> existingParticipants,
            InvitedPersonForEditCommand person,
            int sortKey,
            Organization organization)
        {
            var personFromMain = await _personApiService.GetPersonByOidWithPrivilegesAsync(_plantProvider.Plant,
                person.AzureOid.ToString(), _objectName, _signerPrivileges);
            if (personFromMain != null)
            {
                var existingParticipant = existingParticipants.SingleOrDefault(p => p.Id == person.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        organization,
                        IpoParticipantType.Person,
                        null,
                        personFromMain.FirstName,
                        personFromMain.LastName,
                        personFromMain.Email,
                        new Guid(personFromMain.AzureOid),
                        sortKey,
                        person.RowVersion);
                }
                else
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        organization,
                        IpoParticipantType.Person,
                        null,
                        personFromMain.FirstName,
                        personFromMain.LastName,
                        personFromMain.UserName,
                        personFromMain.Email,
                        new Guid(personFromMain.AzureOid),
                        sortKey));
                }
            }
            else
            {
                throw new IpoValidationException($"Person does not have required privileges to be the {organization} participant.");
            }
        }

        private void AddExternalParticipant(
            Invitation invitation,
            IEnumerable<ParticipantsForEditCommand> participantsWithExternalEmail,
            IList<Participant> existingParticipants)
        {
            foreach (var participant in participantsWithExternalEmail)
            {
                var existingParticipant =
                    existingParticipants.SingleOrDefault(p => p.Id == participant.InvitedExternalEmailToEdit.Id);
                if (existingParticipant != null)
                {
                    invitation.UpdateParticipant(
                        existingParticipant.Id,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        participant.InvitedExternalEmailToEdit.Email,
                        null,
                        participant.SortKey,
                        participant.InvitedExternalEmailToEdit.RowVersion);
                }
                else
                {
                    invitation.AddParticipant(new Participant(
                        _plantProvider.Plant,
                        participant.Organization,
                        IpoParticipantType.Person,
                        null,
                        null,
                        null,
                        null,
                        participant.InvitedExternalEmailToEdit.Email,
                        null,
                        participant.SortKey));
                }
            }
        }
    }
}
