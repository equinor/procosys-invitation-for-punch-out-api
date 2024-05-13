namespace Equinor.ProCoSys.IPO.MessageContracts.Invitation;

public interface IInvitationUpdatedEventV1 : IInvitation
{
    //What to do about update properties, what are these?
    //CompletedAt/By, AcceptedAt/By?
    //ModifiedAtUtc
    //ModifiedBy er ikke med i spec fordi Peter sa det...
}
