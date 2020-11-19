using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public interface IInvitationHelper
    {
        Task<string> GetProjectNameAsync(int invitationId);
    }
}
