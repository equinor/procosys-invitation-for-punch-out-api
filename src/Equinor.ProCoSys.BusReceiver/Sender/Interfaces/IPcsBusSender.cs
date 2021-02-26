using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Equinor.ProCoSys.PcsBus.Sender.Interfaces
{
    public interface IPcsBusSender
    {
        Task SendAsync(string topic, Message message);
        Task CloseAllAsync();
    }
}
