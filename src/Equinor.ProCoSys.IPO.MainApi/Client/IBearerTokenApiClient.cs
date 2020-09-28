using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface IBearerTokenApiClient
    {
        Task<T> TryQueryAndDeserializeAsync<T>(string url);
        Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders=null);
        Task PutAsync(string url, HttpContent content);
    }
}
