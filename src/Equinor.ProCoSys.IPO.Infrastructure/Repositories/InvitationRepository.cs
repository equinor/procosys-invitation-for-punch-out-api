using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class InvitationRepository : RepositoryBase<Invitation>, IInvitationRepository
    {
        public InvitationRepository(IPOContext context)
            : base(context, context.Invitations, 
                context.Invitations.Include(x => x.Participants).Include(x => x.McPkgs).Include(x => x.CommPkgs))
        {
        }

        public void RemoveCommPkg(CommPkg commPkg)
            => _context.CommPkgs.Remove(commPkg);    
        
        public void RemoveMcPkg(McPkg mcPkg)
            => _context.McPkgs.Remove(mcPkg);

        public void RemoveParticipant(Participant participant)
            => _context.Participants.Remove(participant);
    }
}
