using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.WebApi.Synchronization
{
    public interface ICertificateEventProcessorService
    {
        Task ProcessCertificateEventAsync(string messageJson);
    }
}
