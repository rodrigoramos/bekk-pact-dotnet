using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace nPact.Samples.Consumer
{
    class Program
    {
        const string ProvierBaseUrl = "http://localhost:5000/api";

        static async Task Main(string[] args)
        {
            using var httpClient = new HttpClient();

            var name = "Carlos";

            var response = (await httpClient.GetAsync($"{ProvierBaseUrl}/hello/{name}"))
                .EnsureSuccessStatusCode();

            var message = await response.Content.ReadAsStringAsync();

            Console.WriteLine(message);
        }
    }
}
