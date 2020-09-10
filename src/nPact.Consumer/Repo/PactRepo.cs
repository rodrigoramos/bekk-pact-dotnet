using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using nPact.Common;
using nPact.Common.Contracts;
using nPact.Common.Exceptions;
using nPact.Common.Extensions;
using nPact.Consumer.Contracts;
using nPact.Consumer.Rendering;

namespace nPact.Consumer.Repo
{
    class PactRepo : nPact.Common.Utils.PactRepoBase
    {
        public PactRepo(IConfiguration configuration): base(configuration)
        {
        }

        public async Task Put(IPactDefinition pact)
        {
            await PutPacts(pact ,new PactJsonRenderer(pact).ToString());
        }

        private async Task PutPacts(IPactPathMetadata metadata, string payload)
        {
            var broker = PublishToBroker(metadata, payload);
            var filesystem = PublishToFilesystem(metadata, payload);
            await Task.WhenAll(new[]{broker, filesystem});
        }

        private async Task PublishToFilesystem(IPactPathMetadata metadata, string payload)
        {
            if(Configuration.PublishPath == null) return;
            var folder = Path.Combine(Configuration.PublishPath, "pacts", metadata.Provider, metadata.Consumer);
            var filename = $"{metadata.Provider}_{metadata.Consumer}_{metadata.Version}.json";
            var filePath = Path.Combine(folder, filename);
            Directory.CreateDirectory(folder);
            using(var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                var encoded = Encoding.UTF8.GetBytes(payload);
                await file.WriteAsync(encoded, 0, encoded.Length);
            }
            Configuration.LogSafe(LogLevel.Info, $"Saved pact to {filePath}.");
        }
        private async Task PublishToBroker(IPactPathMetadata metadata, string payload)
        {
            if(Configuration.BrokerUri == null) return;
            var url = $"/pacts/provider/{metadata.Provider}/consumer/{metadata.Consumer}/version/{metadata.Version}";
            var client = BrokerClient;
            HttpResponseMessage result;
            try
            {
                result = await client.PutAsync(url, new StringContent(payload, Encoding.UTF8, "application/json"));
                Configuration.LogSafe(LogLevel.Scarce, $"Pact published to broker at {url}.");
            }
            catch(Exception e)
            {
                var exception = e.InnerException??e;
                throw new PactException($"Error when connecting to broker <{BuildUri(url)}>: {exception.Message}", exception);
            }
            if(!result.IsSuccessStatusCode)
            {
                Configuration.LogSafe(LogLevel.Error, $"Broker replied with {(int)result.StatusCode}: {result.ReasonPhrase}");
                if(result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Configuration.LogSafe(LogLevel.Error, "The broker requires username and password. Please verify these configuration values.");
                }
                Configuration.LogSafe(LogLevel.Verbose, await result.Content.ReadAsStringAsync());
                throw new PactRequestException("Couldn't put pact to broker.", result);
            }
            Configuration.LogSafe(LogLevel.Info, $"Uploaded pact to {BuildUri(url)}.");
        }
    }
}