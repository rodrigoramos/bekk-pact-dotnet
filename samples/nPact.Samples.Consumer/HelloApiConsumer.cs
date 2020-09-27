using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace nPact.Samples.Consumer
{
    public class HelloApiConsumer
    {
        public const string HelloApiBaseUrl = "http://localhost:5001/";

        public async Task<string> SayHello(string name = "World")
        {
            var url = $"{HelloApiBaseUrl}api/hello/{name}";

            var httpClient = new HttpClient();

            var httpResponse = await httpClient.GetAsync(url);

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
