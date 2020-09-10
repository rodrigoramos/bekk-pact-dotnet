using System.Net.Http;
using System.Threading.Tasks;

namespace nPact.Provider
{
    public class HttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        protected HttpClientWrapper()
        {
        }

        public HttpClientWrapper(HttpClient httpClient)
            : this()
            => _httpClient = httpClient;

        public virtual async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            => await _httpClient.SendAsync(request);
    }
}