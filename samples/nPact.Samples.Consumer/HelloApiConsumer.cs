using System.Net.Http;
using System.Threading.Tasks;

namespace nPact.Samples.Consumer
{
    public class HelloApiConsumer
    {
        public const string HelloApiBaseUrl = "https://localhost:5001/";

        public async Task<string> SayHello(string name = "World")
        {
            var url = $"{HelloApiBaseUrl}api/hello/{name}";

            var httpClient = new HttpClient();

            var result = await httpClient.GetStringAsync(url);

            return result;
        }
    }
}
