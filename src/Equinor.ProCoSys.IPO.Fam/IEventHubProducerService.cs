namespace Equinor.ProCoSys.IPO.Fam;

public interface IEventHubProducerService
{
    Task SendDataAsync<T>(IEnumerable<T> data);
}
