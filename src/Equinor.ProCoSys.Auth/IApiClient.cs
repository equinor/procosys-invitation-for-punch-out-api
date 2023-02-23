using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Auth
{
    public interface IApiClient
    {
        Task<T> TryQueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null);
        Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null);
        Task PutAsync(string url, HttpContent content);
        Task PostAsync(string url, HttpContent content);
    }
}
