using System.Collections.Generic;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.IPO.ForeignApi.Client
{
    public interface ILibraryApiClient
    {
        Task<T> TryQueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null);
        Task<T> QueryAndDeserializeAsync<T>(string url, List<KeyValuePair<string, string>> extraHeaders = null);
    }
}
