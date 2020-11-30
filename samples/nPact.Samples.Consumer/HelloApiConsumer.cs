using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace nPact.Samples.Consumer
{
    public class HelloApiConsumer
    {
        private readonly HttpClient _httpClient;

        public HelloApiConsumer(HttpClient httpClient) => this._httpClient = httpClient;

        public async Task<string> SayHello(string name = "World")
        {
            var url = $"api/hello/{name}";

            var httpResponse = await _httpClient.GetAsync(url);

            var contentStream = await httpResponse.EnsureSuccessStatusCode()
                .Content
                .ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync<HelloMessage>(contentStream, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result.Message;
        }

        public class HelloMessage
        {
            public string Message { get; set; }
        }
    }
}
